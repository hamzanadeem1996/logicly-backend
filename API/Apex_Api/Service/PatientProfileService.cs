using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using Apex.DataAccess.Response;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using StackExchange.Profiling.Internal;
using static Apex_Api.Common;
using static Apex.DataAccess.Utility;

namespace Apex_Api.Service
{
    public class PatientProfileService
    {
        #region
        public object Save(PatientProfile patientProfile, User me)
        {
            var newAssigneeids = new List<int>
            {
                patientProfile.OT,
                patientProfile.PT,
                patientProfile.SLP,
                patientProfile.SN,
                patientProfile.AID,
                patientProfile.MSW,
                //patientProfile.AddedBy,
                patientProfile.TeamLeader
            };

            var patient = Instances.PatientProfileInst.Get(patientProfile.Id, me.Id);
            var oldAssigneeIds = new List<int>();
            if (patient != null)
            {
                oldAssigneeIds.Add(patient.OT);
                oldAssigneeIds.Add(patient.PT);
                oldAssigneeIds.Add(patient.SLP);
                oldAssigneeIds.Add(patient.SN);
                oldAssigneeIds.Add(patient.AID);
                oldAssigneeIds.Add(patient.MSW);
                oldAssigneeIds.Add(patient.TeamLeader);
                //oldAssigneeids.Add(patient.AddedBy);
            }

            newAssigneeids.RemoveAll(x => x <= 0);
            oldAssigneeIds.RemoveAll(x => x <= 0);

            //WE NEED TO ASSIGN THE VISITS TO THESE NEW PEOPLE NOW
            var assigneeIds = patient != null ? newAssigneeids.Except(oldAssigneeIds).ToList() : newAssigneeids;
            //WE NEED TO DELETE VISITS FOR THESE NEW PEOPLE NOW
            var assigneeIdsRemoved = patient != null ? oldAssigneeIds.Except(newAssigneeids).ToList() : oldAssigneeIds;

            patientProfile.Eoc = patientProfile.Admission.AddDays(59);

            // if Admission date is thursday,friday,saturday we allow for 10 weeks and evalution will be on monday next week.
            var admissionDay = patientProfile.Admission.DayOfWeek.ToString();
            if (admissionDay == Constant.THURSDAY || admissionDay == Constant.FRIDAY ||
                admissionDay == Constant.SATURDAY)
                patientProfile.Evaluation = Utility.GetNextWeekday(patientProfile.Admission, DayOfWeek.Monday);
            else
                patientProfile.Evaluation = patientProfile.Admission.AddDays(2);

            // Count 30 day eval based on evaluation date
            patientProfile.ThirtyDaysRelEval = patientProfile.Evaluation.AddDays(29);

            //1. If the frequency is 1w1, then if the evaluation is updated then the discharge date
            //should update as well.
            //2.If the Admission date is updated, check if frequency exist or not and update other
            //values accordingly

            if (patient != null && patient.Frequency == "1w1" && patient.Id > 0)
                patientProfile.Discharge = Utility.GetNextWeekday(patientProfile.Evaluation,
                    DayOfWeek.Saturday);
            else if (patient != null && patient.Frequency != "1w1" && patient.Frequency != "N/A"
                     && patient.Id > 0)
            {
                patientProfile.Discharge = patient.Discharge;
            }
            else
                patientProfile.Discharge = patientProfile.Admission.AddDays(59);

            //DELETE ALL EXISTING DATES
            Instances.PatientProfileInst.DeleteByPatientid(patientProfile.Id, "RoutineVisit", assigneeIdsRemoved);

            //get city name from lat and long
            var loc = GoogleMapsService.GetLocationInfo(patientProfile.Lat, patientProfile.Long);
            if (!loc.City.IsNullOrWhiteSpace()) patientProfile.CityName = loc.City;


            // Update patient profile
            var result = Instances.PatientProfileInst.Save(patientProfile, me.Id);

            // Add Default recertification//
            var getRecert = Instances.RecertificationInst.GetRecertifications(patientProfile.Id)
                .FirstOrDefault();
            if (getRecert == null || getRecert.Id == 0)
            {
                // Add Default recertification//
                var defaultRecert = new Recertification
                {
                    PatientId = patientProfile.Id,
                    RecertificationDate = patientProfile.Admission
                };
                getRecert = Instances.RecertificationInst.Save(defaultRecert, patientProfile.AddedBy);

                var dateTimes = new Dictionary<DateTime, string>
                {
                    {patientProfile.Admission.AddDays(59).AddSeconds(4), "Recert"},
                    {patientProfile.Eoc.AddSeconds(5), "Eoc"},
                };
                foreach (var kvp in dateTimes)
                {
                    var date = new PatientDate
                    {
                        PatientId = patientProfile.Id,
                        ClinicianId = patientProfile.AddedBy,
                        RecertId = getRecert.Id,
                        Status = "N/A",
                        PatientDates = kvp.Key,
                        Type = kvp.Value
                    };
                    Instances.PatientProfileInst.PatientDateSave(date);
                }
            }

            if (getRecert.Id > 0)
            {
                foreach (var item in assigneeIds.Distinct())
                {
                    if (item > 0)
                    {
                        var date = new Dictionary<DateTime, string>
                        {
                            {patientProfile.Evaluation.AddSeconds(1), "Evaluation"},
                            {patientProfile.ThirtyDaysRelEval.AddSeconds(2), "30DRE"},
                        };
                        foreach (var kvp in date)
                        {
                            var generatedate = new PatientDate
                            {
                                PatientId = patientProfile.Id,
                                ClinicianId = item,
                                RecertId = getRecert.Id,
                                Status = "N/A",
                                PatientDates = kvp.Key,
                                Type = kvp.Value
                            };
                            Instances.PatientProfileInst.PatientDateSave(generatedate);
                        }
                    }
                }
            }

            // // save patient lock status
            // var getPatientLockStatus = Instances.VisitLockInst.Get(patientProfile.Id);
            // if (getPatientLockStatus == null)
            // {
            //     var visitLockStatus = new VisitLockStatus
            //     {
            //         PatientId = patientProfile.Id,
            //         IsLocked = false,
            //     };
            //     Instances.VisitLockInst.Save(visitLockStatus);
            // }

            return result;
        }
        #endregion

        #region
        public List<Schedule> GetSchedule(List<int> Patientids, List<int> userid, DateTime startdate, string query = "",
            bool filterByDate = true, string status = "Active", int agencyId = 0, string role = "",
            string actualDate = "", bool automatic = false)
        {
            var schedulelist = new List<Schedule>();
            var t = new List<VType>();
            var newlis = new VType();

            // Get All Patient List
            var SelectedPatients =
                Common.Instances.PatientProfileInst.GetAll(Patientids, userid, 1, 1000, query, status
                    , "validaddress", agencyId);

            var type = new VType();
            var schedule = new Schedule();

            if (role != "OTA" && role != "PTA" && role != "AID" && role != "MSW")
            {
                SelectedPatients.Items = SelectedPatients.Items.Where(x => x.Frequency != "N/A").ToList();
            }

            var sttDate = new DateTime(startdate.Year, startdate.Month, 1);
            var edDate = sttDate.AddMonths(1).AddDays(-1);

            var getAllPatientDateByPatientId = new List<PatientDate>();
            foreach (var item in SelectedPatients.Items)
            {
                // Get  Patient Dates in PatientDates table by using patient id
                getAllPatientDateByPatientId = Common.Instances.PatientProfileInst
                    .PatientDates(userid, startdate.Date.ToString("yyyy-MM-dd"),
                        startdate.AddDays(6).ToString("yyyy-MM-dd"), item.Id, filterByDate, "Discharged", agencyId)
                    .ToList();

                bool weeklyExist = getAllPatientDateByPatientId.Any(x => x.FrequencyType == "W");

                if (weeklyExist)
                    getAllPatientDateByPatientId = getAllPatientDateByPatientId.Where(x =>
                    x.FrequencyType == "W" || x.FrequencyType == null || x.FrequencyType == "N/A").ToList();
                 else
                    getAllPatientDateByPatientId = Common.Instances.PatientProfileInst
                        .PatientDates(userid, sttDate.Date.ToString("yyyy-MM-dd"),
                            edDate.ToString("yyyy-MM-dd"), item.Id, filterByDate, "Discharged", agencyId).Where(x =>
                            x.FrequencyType == "M" || x.FrequencyType == null || x.FrequencyType == "N/A")
                        .ToList();


                var recertRange = Common.Instances.PatientProfileInst
                    .PatientDates(userid, Convert.ToDateTime(actualDate).AddDays(1).ToString("yyyy-MM-dd"),
                        Convert.ToDateTime(actualDate).AddDays(4).ToString("yyyy-MM-dd"), item.Id, filterByDate,
                        "Discharged",
                        agencyId).Where(x => x.Type == "Recert").ToList();

                var NewPatientDateDetails = getAllPatientDateByPatientId.Concat(recertRange);

                var GetType = NewPatientDateDetails
                    .Select(i => new
                    {
                        i.Status,
                        i.Id,
                        i.Type,
                        i.PatientDates,
                        i.ClinicianId,
                        i.RecertId,
                        i.CertEndDate,
                        i.CertStartDate,
                        i.IsAddedToSchedule,
                        Count = i.Type.Count()
                    }).OrderByDescending(x => x.Count).ToList();

                GetType = GetType.Where(y =>
                        Utility.DateInRange((Convert.ToDateTime(actualDate).Date), y.CertStartDate, y.CertEndDate))
                    .Distinct().ToList();

                bool containRecert = GetType.Any(x => x.Type == "Recert");
                if (containRecert)
                {
                    var recert = GetType.Where(x => x.Type == "Recert").ToList();
                    var isRecert = recert.Where(y => Utility.DateInRange((Convert.ToDateTime(actualDate).Date),
                        y.CertEndDate.AddDays(-4), y.CertEndDate)).ToList();
                    if (isRecert.Count == 0)
                        GetType.Remove(GetType.Find(x => x.Type == "Recert"));
                }

                if (GetType.Count > 0)
                {
                    schedule.AllDay = false;
                    schedule.Title = $"{item.FirstName} {item.LastName}";
                    schedule.PatientId = item.Id;
                    schedule.CityName = item.CityName;
                    schedule.Distance = "N/A";
                   
                    // TAsk : a patient who has 2x a week visit has a 30 day re eval due. But the app creates 3 bubbles but it should be 2.Here, the 30DRE takes the place of the routine visit.
                    //This tells us the frequency is 3x a week when in reality it's only 2.
                    //////////////////
                    ///
                    //if(role != "OTA" || role!= "PTA" || role != "AID")
                    //{
                    int routineVisitCount = 0;
                    int ThirtyDRE = 0;
                    int DisCharge = 0;
                    int recert = 0;
                    int Evaluation = 0;
                    int hold = 0;
                    foreach (var itm in GetType)
                    {
                        if (itm.Type == "RoutineVisit")
                        {
                            routineVisitCount++;
                        }
                        else if (itm.Type == "30DRE")
                        {
                            ThirtyDRE++;
                        }
                        else if (itm.Type == "Recert")
                        {
                            recert++;
                        }
                        else if (itm.Type == "Discharge")
                        {
                            DisCharge++;
                        }
                        else if (itm.Type == "Hold")
                        {
                            hold++;
                        }
                        else if (itm.Type == "Evaluation")
                        {
                            Evaluation++;
                        }
                        else
                        {
                        }
                    }

                    if (routineVisitCount == 2 && DisCharge > 0 && Evaluation > 0)
                    {
                        GetType.RemoveAt(0);
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 1 && DisCharge == 1 && Evaluation == 1)
                    {
                        GetType.RemoveAt(0);
                        GetType.RemoveAt(1);
                    }
                    else if (routineVisitCount == 3 && DisCharge > 0 && Evaluation > 0)
                    {
                        GetType.RemoveAt(0);
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 4 && DisCharge > 0 && Evaluation > 0)
                    {
                        GetType.RemoveAt(0);
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 5 && DisCharge > 0 && Evaluation > 0)
                    {
                        GetType.RemoveAt(0);
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 6 && DisCharge > 0 && Evaluation > 0)
                    {
                        GetType.RemoveAt(0);
                        GetType.RemoveAt(0);
                    }

                    else if (routineVisitCount >= 7 && DisCharge > 0 && Evaluation > 0)
                    {
                        GetType.RemoveAt(0);
                        //GetType.RemoveAt(0);
                    }

                    else if (routineVisitCount == 2 && (ThirtyDRE > 0 || recert > 0 || DisCharge > 0 || Evaluation > 0))
                    {
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 3 && (ThirtyDRE > 0 || recert > 0 || DisCharge > 0 || Evaluation > 0))
                    {
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 4 && (ThirtyDRE > 0 || recert > 0 || DisCharge > 0 || Evaluation > 0))
                    {
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 5 && (ThirtyDRE > 0 || recert > 0 || DisCharge > 0 || Evaluation > 0))
                    {
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 6 && (ThirtyDRE > 0 || recert > 0 || DisCharge > 0 || Evaluation > 0))
                    {
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount >= 7 && (ThirtyDRE > 0 || recert > 0 || DisCharge > 0 || Evaluation > 0))
                    {
                        GetType.RemoveAt(0);
                    }
                    else if (routineVisitCount == 1 && GetType.Count == 2)
                    {
                        GetType.RemoveAt(0);
                    }

                    else if (routineVisitCount == 1 && DisCharge == 1 && recert == 1)
                    {
                        GetType.RemoveAt(0);
                    }

                    else if (routineVisitCount == 1 && DisCharge == 1 && ThirtyDRE == 1)
                    {
                        GetType.RemoveAt(0);
                    }

                    else if (hold == 1 && Evaluation == 1)
                    {
                        GetType.Remove(GetType.Find(x => x.Type == "Evaluation"));
                    }

                    else if (hold == 1 && ThirtyDRE == 1)
                    {
                        GetType.Remove(GetType.Find(x => x.Type == "30DRE"));
                    }

                    //  End  ////////////////////////

                    // count 0
                    routineVisitCount = 0;
                    ThirtyDRE = 0;
                    recert = 0;
                    hold = 0;
                    // end count 0
                    //}

                    foreach (var item1 in GetType)
                    {
                        if (item1.Type == "Evaluation" && role != "OTA" && role != "PTA" && role != "AID" &&
                            role != "MSW")
                        {
                            schedule.Start = item1.PatientDates.ToString("yyyy-MM-dd");

                            schedule.color = item1.IsAddedToSchedule == true ? Constant.COLORGREY : "#BA96D7";

                            schedule.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;

                            schedule.colorType = "E";
                            schedule.RecertId = item1.RecertId;
                            type.Id = item1.Id; // PATIENT DATES TABLE PRIMARY KEY
                            type.ClinicianId = item1.ClinicianId;
                            type.Visitcolor = item1.IsAddedToSchedule == true &&
                                              item1.Status == Constant.VISITSTATUSCOMPLETED
                                ? Constant.COLORCOMPLETED
                                : item1.IsAddedToSchedule == true
                                    ? Constant.COLORGREY
                                    : item1.Status == "MISSED"
                                        ? Constant.COLORMISSED
                                        : "#BA96D7";

                            type.VisitTypeCode = "E";

                            type.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            type.RecertId = item1.RecertId;
                            type.CertStartDate = item1.CertStartDate;
                            type.CertEndDate = item1.CertEndDate;
                            type.IsCompleted = item1.Status == Constant.VISITSTATUSCOMPLETED ? true : false;
                            type.IsPrimary = false;
                            type.IsCombined = false;
                            type.RoutineVisitDate = item1.PatientDates.Date;
                            type.Sortby = 0;
                            t.Add(type);
                            type = new VType();
                        }

                        if (item1.Type == "Discharge" && role != "OTA" && role != "PTA" && role != "AID" &&
                            role != "MSW")
                        {
                            schedule.Start = item1.PatientDates.ToString("yyyy-MM-dd");
                            schedule.color = item1.IsAddedToSchedule == true ? Constant.COLORGREY : "#EDD300";
                            schedule.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            schedule.colorType = "D";
                            schedule.RecertId = item1.RecertId;
                            type.Id = item1.Id; // PATIENT DATES TABLE PRIMARY KEY
                            type.ClinicianId = item1.ClinicianId;
                            type.Visitcolor = item1.IsAddedToSchedule == true &&
                                              item1.Status == Constant.VISITSTATUSCOMPLETED
                                ? Constant.COLORCOMPLETED
                                : item1.IsAddedToSchedule == true
                                    ? Constant.COLORGREY
                                    : item1.Status == "MISSED"
                                        ? Constant.COLORMISSED
                                        : "#EDD300";

                            type.VisitTypeCode = "D";
                            type.RecertId = item1.RecertId;
                            type.CertStartDate = item1.CertStartDate;
                            type.CertEndDate = item1.CertEndDate;
                            type.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            type.IsCompleted = item1.Status == Constant.VISITSTATUSCOMPLETED ? true : false;
                            type.RoutineVisitDate = item1.PatientDates.Date;
                            type.Sortby = 6;
                            type.IsPrimary = false;
                            type.IsCombined = false;
                            t.Add(type);
                            type = new VType();
                        }

                        if (item1.Type == "30DRE" && role != "OTA" && role != "PTA" && role != "AID" && role != "MSW")
                        {
                            schedule.Start = item1.PatientDates.ToString("yyyy-MM-dd");
                            schedule.color = item1.IsAddedToSchedule == true ? Constant.COLORGREY : "#759AE0";
                            schedule.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            schedule.colorType = "30";
                            schedule.RecertId = item1.RecertId;
                            type.Id = item1.Id; // PATIENT DATES TABLE PRIMARY KEY
                            type.ClinicianId = item1.ClinicianId;
                            type.Visitcolor = item1.IsAddedToSchedule == true &&
                                              item1.Status == Constant.VISITSTATUSCOMPLETED
                                ? Constant.COLORCOMPLETED
                                : item1.IsAddedToSchedule == true
                                    ? Constant.COLORGREY
                                    : item1.Status == "MISSED"
                                        ? Constant.COLORMISSED
                                        : "#759AE0";

                            type.VisitTypeCode = "30";
                            type.RecertId = item1.RecertId;
                            type.CertStartDate = item1.CertStartDate;
                            type.CertEndDate = item1.CertEndDate;
                            type.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            type.IsCompleted = item1.Status == Constant.VISITSTATUSCOMPLETED ? true : false;
                            type.IsPrimary = false;
                            type.IsCombined = false;
                            type.RoutineVisitDate = item1.PatientDates.Date;
                            type.Sortby = 3;
                            t.Add(type);
                            type = new VType();
                        }

                        if (item1.Type == "Recert" && role != "OTA" && role != "PTA" && role != "AID" && role != "MSW")
                        {
                            schedule.Start = item1.PatientDates.ToString("yyyy-MM-dd");
                            schedule.color = item1.IsAddedToSchedule == true ? Constant.COLORGREY : "#D40000";
                            schedule.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            schedule.colorType = "R";
                            schedule.RecertId = item1.RecertId;
                            type.Id = item1.Id; // PATIENT DATES TABLE PRIMARY KEY
                            type.ClinicianId = item1.ClinicianId;
                            type.Visitcolor = item1.IsAddedToSchedule == true &&
                                              item1.Status == Constant.VISITSTATUSCOMPLETED
                                ? Constant.COLORCOMPLETED
                                : item1.IsAddedToSchedule == true
                                    ? Constant.COLORGREY
                                    : item1.Status == "MISSED"
                                        ? Constant.COLORMISSED
                                        : "#D40000";

                            type.VisitTypeCode = "R";
                            type.RecertId = item1.RecertId;
                            type.CertStartDate = item1.CertStartDate;
                            type.CertEndDate = item1.CertEndDate;
                            type.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            type.IsCompleted = item1.Status == Constant.VISITSTATUSCOMPLETED ? true : false;
                            type.IsPrimary = false;
                            type.IsCombined = false;
                            type.RoutineVisitDate = item1.PatientDates.Date;
                            type.Sortby = 5;
                            t.Add(type);
                            type = new VType();
                        }

                        if (item1.Type == "Hold")
                        {
                            schedule.Start = item1.PatientDates.ToString("yyyy-MM-dd");
                            schedule.color = Constant.COLORGREY;
                            schedule.IsDisabled = true;
                            schedule.colorType = "H";
                            schedule.RecertId = item1.RecertId;
                            type.RecertId = item1.RecertId;
                            type.CertStartDate = item1.CertStartDate;
                            type.CertEndDate = item1.CertEndDate;
                            type.Id = item1.Id; // PATIENT DATES TABLE PRIMARY KEY
                            type.ClinicianId = item1.ClinicianId;
                            type.Visitcolor = Constant.COLORGREY;
                            type.VisitTypeCode = "H";
                            type.IsDisabled = true;
                            type.IsCompleted = item1.Status == Constant.VISITSTATUSCOMPLETED ? true : false;
                            type.IsPrimary = false;
                            type.IsCombined = false;
                            type.RoutineVisitDate = item1.PatientDates.Date;
                            type.Sortby = 4;
                            t.Add(type);
                            type = new VType();
                        }

                        if (item1.Type == "RoutineVisit")
                        {
                            schedule.Start = item1.PatientDates.ToString("yyyy-MM-dd");
                            schedule.color = item1.IsAddedToSchedule == true ? Constant.COLORGREY : "#7db885";
                            schedule.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            schedule.colorType = "RV";
                            schedule.RecertId = item1.RecertId;
                            type.RecertId = item1.RecertId;
                            type.CertStartDate = item1.CertStartDate;
                            type.CertEndDate = item1.CertEndDate;
                            type.Id = item1.Id; // PATIENT DATES TABLE PRIMARY KEY
                            type.ClinicianId = item1.ClinicianId;
                            type.Visitcolor = item1.IsAddedToSchedule == true &&
                                              item1.Status == Constant.VISITSTATUSCOMPLETED
                                ? Constant.COLORCOMPLETED
                                : item1.IsAddedToSchedule == true
                                    ? Constant.COLORGREY
                                    : item1.Status == "MISSED"
                                        ? Constant.COLORMISSED
                                        : "#7db885";
                            type.VisitTypeCode = "RV";
                            type.IsDisabled = schedule.color == Constant.COLORGREY ? true : false;
                            type.IsCompleted = item1.Status == Constant.VISITSTATUSCOMPLETED ? true : false;
                            type.IsPrimary = false;
                            type.IsCombined = false;
                            type.RoutineVisitDate = item1.PatientDates.Date;
                            type.Sortby = 2;
                            t.Add(type);
                            type = new VType();
                        }
                    }

                    //Caseload, if a patient discharge is scheduled then the patient stops showing for further weeks even if visits exists for those weeks 
                    var DischargeSchedule =
                        new PatientDateRepo().GetDischargeIsScheduled(schedule.PatientId, userid[0],schedule.RecertId);

                    if (DischargeSchedule.Id > 0 && DischargeSchedule.IsAddedToSchedule == true && t.Count > 0)
                        t = t.Where(x => x.RoutineVisitDate.Date <= DischargeSchedule.PatientDates).ToList();

                    schedule.vsttype = t.OrderBy(x => x.Sortby).ToList();

                    bool CombineVisit = schedule.vsttype.Any(x => x.VisitTypeCode == "30" || x.VisitTypeCode == "R");
                    bool isDischarge = schedule.vsttype.Any(y => y.VisitTypeCode == "D");

                    if (CombineVisit && isDischarge)
                    {
                        schedule.vsttype[schedule.vsttype.FindIndex(ind => ind.VisitTypeCode == "30"
                                                                           || ind.VisitTypeCode == "R")].IsPrimary =
                            true;
                        schedule.vsttype[
                                schedule.vsttype.FindIndex(ind =>
                                    ind.VisitTypeCode == "30" || ind.VisitTypeCode == "R")]
                            .IsCombined = true;
                        schedule.vsttype[schedule.vsttype.FindIndex(ind => ind.VisitTypeCode == "D")].IsCombined = true;
                    }

                    if (automatic)
                        schedule.vsttype = schedule.vsttype.Where(x => !x.IsDisabled).ToList();

                    t = new List<VType>();

                    schedulelist.Add(schedule);
                    schedule = new Schedule();
                }
            }


            var scheduleListOrderBy = schedulelist.Where(x => x.vsttype.Count > 0).OrderBy(x => x.CityName).ToList();
            return scheduleListOrderBy;
        }
        #endregion

        private void DeleteExistingData(PatientScheduleSave patientSchedule, int clinicianId)
        {
            //DELETE EXISTING DATA

            Instances.patientProfileService.DeleteByPatientId(patientSchedule.PatientId,
                patientSchedule.RecertId, clinicianId);

            new PatientDateRepo().DeleteDischarge(patientSchedule.PatientId, patientSchedule.RecertId, clinicianId);

            Instances.patientProfileService.DeletePatientDatesByPatientid(patientSchedule.PatientId,
                patientSchedule.RecertId, "RoutineVisit", clinicianId);

            //Remove Check Do not delete the scheduled visits from the full calendar, when the frequency is updated for a patient

            Instances.VisitScheduleRepoInst.DeletePatientVisitSchedule(patientSchedule.PatientId,
            patientSchedule.RecertId, "RV", clinicianId);

            Instances.patientProfileService.UpdateEval(patientSchedule.PatientId, patientSchedule.RecertId,
                clinicianId, "N/A");
        }

        private void DeleteExistingDisChargeData(PatientScheduleSave patientSchedule, int clinicianId)
        {
            //DELETE EXISTING DATA
            new PatientDateRepo().DeleteDischarge(patientSchedule.PatientId, patientSchedule.RecertId, clinicianId);
            Instances.VisitScheduleRepoInst.DeletePatientDischargeVisitSchedule(patientSchedule.PatientId,
        patientSchedule.RecertId, clinicianId);
        }

        public object PatientScheduleSave(PatientScheduleSave patientSchedule, int clinicianId, ApiResponse apiResponse,
            string role = "")
        {
            if (role == "OTA" || role == "PTA" || role == "AID")
                throw new HttpException(402,
                    $"You are not authorized to add a frequency.");

            if (patientSchedule.GeneratedVisitCode == null ||
               patientSchedule.GeneratedVisitCode.Length <= 0)
                throw new HttpException(402,
                    $"Invalid GeneratedVisitCode.");


            var calendar = new Calendar();
            var newdate = new DateTime();

            DateTime eoc;
            DateTime evaluation;

            var patient = Common.Instances.PatientProfile.Get(patientSchedule.PatientId, clinicianId);

            // GET EVALUATION BASED ON CERTIFICATION
            var allEvaluations = new PatientDateRepo().GetMultipleVisits(patientSchedule.PatientId, clinicianId,
                patientSchedule.RecertId, "Evaluation");

            if (allEvaluations.Count > 0)
            {
                evaluation = allEvaluations.FirstOrDefault().PatientDates;
                eoc = patient.Admission.AddDays(59);
            }
            else
            {
                evaluation = new RecertificationService().GetRecertificationbyId(patientSchedule.RecertId,
                    clinicianId, patientSchedule.PatientId).RecertificationDate;
                eoc = evaluation.AddDays(59);
            }

            //EOC LOGIC ENDS HERE

            if (evaluation.Year < 1950)
                throw new HttpException((int)HttpStatusCode.Unauthorized,
                    $"Invalid evaluation.");

            if (eoc.Year < 1950)
                throw new HttpException((int)HttpStatusCode.Unauthorized,
                    $"Invalid Eoc.");

            int weekNo = 0;
            bool allowFreqSave = false;
            var rangeWeek = new WeekRange();

            var GetAllFrequency = new List<PatientSchedule>();

            DateTime weekEndRange = new DateTime();
            if (!patientSchedule.IsFrequencyNew)
            {
                weekEndRange = new PatientDateRepo().GetLastPatientDate(patientSchedule.PatientId,
                clinicianId, patientSchedule.RecertId).LastOrDefault().PatientDates;
            }
                
           foreach (var freq in patientSchedule.GeneratedVisitCode)
           {                  
                patientSchedule.GeneratedVisitCode = freq.Split(',');
                foreach (var item in patientSchedule.GeneratedVisitCode)
                {
                    var unit = new string(item.Where(char.IsLetter).ToArray());
                    var number = item.Split(item.Where(char.IsLetter).ToArray());
                    weekNo = Utility.WeekNumber(weekEndRange);
                    switch (unit.ToUpper())
                    {
                        case "W":
                            rangeWeek = Utility.WeekDateRange(evaluation.Year, (weekNo - 1 + Convert.ToInt32(number.Last())));
                            allowFreqSave = Utility.IsBetween(eoc, rangeWeek.WeekStart, rangeWeek.WeekEnd);
                            if (!allowFreqSave)
                                throw new HttpException(402,
                                    Constant.FREQUENCYLIMITEXCEED);

                            weekEndRange = rangeWeek.WeekEnd;
                            break;

                        case "M":

                            for (var idxx = 0; idxx < Convert.ToInt32(number.Last()); idxx++)
                            {
                                weekEndRange = weekEndRange.Day == 31 ? weekEndRange.AddDays(1) : weekEndRange.Day == 30 ? weekEndRange.AddDays(1) : weekEndRange;

                                var leftdaysinmonth = Utility.GetRemaningDaysInMonth(weekEndRange);
                                if (Convert.ToInt32(number.First()) > leftdaysinmonth)
                                    throw new HttpException((int)HttpStatusCode.Unauthorized,
                                        $"Visit frequency exceeded for month {weekEndRange.ToString("MMMM")}, only {leftdaysinmonth} days in this month.");

                                else if (weekEndRange.AddDays(Convert.ToInt32(number.First())) > eoc)
                                    throw new HttpException(402,
                                        Constant.FREQUENCYLIMITEXCEED);
                              
                                    weekEndRange = weekEndRange.AddDays(leftdaysinmonth - 1);
                            }
                            break;
                    }
                }
           }

            if (!patientSchedule.IsFrequencyNew)
            {
                newdate = Utility.GetNextWeekday(new PatientDateRepo().GetLastPatientDate(patientSchedule.PatientId,
                clinicianId, patientSchedule.RecertId).LastOrDefault().PatientDates, DayOfWeek.Sunday);

                // DELETE DISCHARGE
                DeleteExistingDisChargeData(patientSchedule, clinicianId);
            }

            if (patientSchedule.IsFrequencyNew)
            DeleteExistingData(patientSchedule, clinicianId);

            DeleteExistingDisChargeData(patientSchedule, clinicianId);

            patientSchedule.ClinicianId = clinicianId;

            var newlist = new List<PatientSchedule>();

            var i = patientSchedule.IsFrequencyNew == true ? 1 : 2 ;
            var j = patientSchedule.IsFrequencyNew == true ? 0 : 1;
            var k = patientSchedule.IsFrequencyNew == true ? 0 : 1;

            //var newdate = new DateTime();

            foreach (var item1 in patientSchedule.GeneratedVisitCode)
            {
                var unit = new string(item1.Where(char.IsLetter).ToArray());
                if (string.IsNullOrEmpty(unit)) break;

                var number = item1.Split(item1.Where(char.IsLetter).ToArray());

                patientSchedule.NumberOfUnits = Convert.ToInt32(number.Last());
                patientSchedule.VisitsPerUnit = Convert.ToInt32(number.First());

                var patientSchedule1 = new PatientSchedule
                {
                    PatientId = patientSchedule.PatientId,
                    ClinicianId = patientSchedule.ClinicianId,
                    RecertId = patientSchedule.RecertId,
                    GeneratedVisitCode = item1,
                    NumberOfUnits = patientSchedule.NumberOfUnits,
                    VisitsPerUnit = patientSchedule.VisitsPerUnit,


                    Unit = unit == "m" ? "Month" : "Week",


                    SortIndx = i,
                };
                newlist.Add(patientSchedule1);
                i++;
                k++;
                using (var db = Utility.Database)
                {
                    db.Save(patientSchedule1);
                }


                var isconditionTrue = Utility.GetLeftDaysInWeek(evaluation) >= patientSchedule.VisitsPerUnit;

                if (unit.ToUpper().Trim() == "W")
                    evaluation = Utility.GetLeftDaysInWeek(evaluation) >= patientSchedule.VisitsPerUnit
                        ? evaluation
                        : Utility.GetNextWeekday(evaluation, DayOfWeek.Sunday);

                if (k == 1)
                    Common.Instances.patientProfileService.UpdateEval(patientSchedule.PatientId,
                        patientSchedule.RecertId, clinicianId, unit.ToUpper().Trim());

                if (patientSchedule.NumberOfUnits >= 1 && unit.ToUpper().Trim() == "M")
                {
                    for (var index = 1; index <= patientSchedule.NumberOfUnits; index++)
                    {
                        if (patientSchedule.NumberOfUnits >= 1 && patientSchedule.VisitsPerUnit >= 1)
                        {
                            var monthno = j == 0 ? Convert.ToInt32(evaluation.Month) : Convert.ToInt32(newdate.Month);

                            var rrule = new RecurrencePattern(
                                unit.ToLower() == "m" ? FrequencyType.Daily : FrequencyType.Weekly, 1)
                            {
                                Count = unit.ToLower() == "m" ? patientSchedule.VisitsPerUnit : 0,
                                ByMonth = {monthno},
                            };
                            var Routineevt = new CalendarEvent
                            {
                                Start = j == 0 ? new CalDateTime(evaluation) : new CalDateTime(newdate),
                                RecurrenceRules = new List<RecurrencePattern> {rrule}
                            };
                            calendar.Events.Add(Routineevt);

                            var week = patientSchedule.NumberOfUnits * 7;
                            week = patientSchedule.NumberOfUnits == 7 ? week : week;

                            var month = patientSchedule.NumberOfUnits * 30;

                            var occurrences = j == 0 && unit == "m"
                                ? calendar.GetOccurrences(evaluation.AddDays(-1),
                                    evaluation.AddDays(
                                        evaluation.AddDays(1 - evaluation.Day).AddMonths(1).AddDays(-1).Day -
                                        evaluation.Day))
                                : j == 0 && unit != "m"
                                    ? calendar.GetOccurrences(evaluation, evaluation.AddDays(week))
                                    : j > 0 && unit == "m"
                                        ? calendar.GetOccurrences(newdate.AddDays(-1),
                                            newdate.AddDays(newdate.AddDays(1 - newdate.Day).AddMonths(1).AddDays(-1)
                                                .Day - newdate.Day))
                                        : calendar.GetOccurrences(newdate, newdate.AddDays(week));

                            foreach (var item in occurrences)
                            {
                                var date = new PatientDate
                                {
                                    PatientId = patientSchedule.PatientId,
                                    ClinicianId = patientSchedule.ClinicianId,
                                    RecertId = patientSchedule.RecertId,
                                    ScheduleId = patientSchedule1.Id,
                                    PatientDates = item.Period.StartTime.Date,
                                    Type = "RoutineVisit",
                                    FrequencyType = "M"
                                };
                                if (item.Period.StartTime.Date <= eoc.Date)
                                {
                                    Common.Instances.PatientProfileInst.PatientDateSave(date);
                                    newdate = unit == "m"
                                        ? item.Period.StartTime.Date
                                        : item.Period.StartTime.Date.AddDays(1);
                                    calendar = new Calendar();
                                }
                            }

                            //var disChargeDate = unit == "m"
                            //    ? Utility.GetLastDayOfMonth(newdate)
                            //    : Utility.GetNextWeekday(newdate.AddDays(-1), DayOfWeek.Saturday);

                            var disChargeDate = unit == "m"
                                ? Utility.GetLastDayOfMonth(newdate)
                                : newdate.AddDays(-1);

                            if (eoc.Year != 0001)
                            {
                                Instances.PatientProfileInst.SaveDischargeDate(patientSchedule.PatientId,
                                    patientSchedule.RecertId,
                                    patientSchedule.ClinicianId, disChargeDate >= eoc ? eoc : disChargeDate, "Discharge",
                                "N/A", unit.ToUpper());

                                newdate = disChargeDate >= eoc
                                    ? eoc
                                    : Utility.GetNextWeekday(disChargeDate, DayOfWeek.Sunday);
                            }

                            j++;
                        }
                        else
                        {
                            if (patientSchedule.NumberOfUnits <= 0) continue;
                            for (int idx = 0; idx <= patientSchedule.NumberOfUnits; idx++)
                            {
                                var date = new PatientDate
                                {
                                    PatientId = patientSchedule.PatientId,
                                    ClinicianId = patientSchedule.ClinicianId,
                                    RecertId = patientSchedule.RecertId,
                                    ScheduleId = patientSchedule1.Id,
                                    PatientDates = newdate.Year < 1970
                                        ? evaluation
                                        : Utility.GetNextWeekday(newdate, DayOfWeek.Sunday),
                                    Type = "Hold",
                                    FrequencyType = "M"
                                };

                                if (date.PatientDates <= eoc.Date)
                                {
                                    Common.Instances.PatientProfileInst.PatientDateSave(date);
                                    newdate = date.PatientDates.AddDays(1);
                                    calendar = new Calendar();
                                }

                                idx++;
                                newdate = Utility.GetNextWeekday(newdate, DayOfWeek.Sunday);
                            }

                            j++;
                        }
                    }
                }
                else
                {
                    if (patientSchedule.NumberOfUnits >= 1 && patientSchedule.VisitsPerUnit >= 1)
                    {
                        var rrule = new RecurrencePattern(
                            unit.ToLower() == "m" ? FrequencyType.Daily : FrequencyType.Weekly, 1)
                        {
                            Count = unit.ToLower() == "m" ? patientSchedule.VisitsPerUnit + 1 : 0,
                            ByWeekNo = unit.ToLower() == "w"
                                ? new List<int> {patientSchedule.NumberOfUnits}
                                : new List<int> {0},
                            ByDay = Utility.GetDaysOfWeek(patientSchedule.VisitsPerUnit)
                        };

                        var routineevt = new CalendarEvent
                        {
                            Start = j == 0
                                ? new CalDateTime(evaluation.AddDays(-1))
                                : new CalDateTime(newdate.AddDays(-1)),
                            RecurrenceRules = new List<RecurrencePattern> {rrule}
                        };
                        calendar.Events.Add(routineevt);

                        var week = patientSchedule.NumberOfUnits * 7;
                        week = patientSchedule.NumberOfUnits == 7 ? week + 1 : week;

                        var month = patientSchedule.NumberOfUnits * 30;

                        HashSet<Occurrence> occurrences;
                        switch (j)
                        {
                            case 0 when unit == "m":
                                occurrences = calendar.GetOccurrences(evaluation.AddDays(-1),
                                    evaluation.AddDays(
                                        evaluation.AddDays(1 - evaluation.Day).AddMonths(1).AddDays(-1).Day -
                                        evaluation.Day));
                                break;
                            case 0 when unit != "m":
                                occurrences = calendar.GetOccurrences(evaluation.AddDays(-1), evaluation.AddDays(week));
                                break;
                            default:
                            {
                                if (j > 0 && unit == "m")
                                    occurrences = calendar.GetOccurrences(newdate.AddDays(-1),
                                        newdate.AddDays(newdate.AddDays(1 - newdate.Day).AddMonths(1).AddDays(-1).Day -
                                                        newdate.Day));
                                else
                                    occurrences = calendar.GetOccurrences(newdate.AddDays(-1), newdate.AddDays(week));
                                break;
                            }
                        }

                        if (!isconditionTrue && occurrences.Count >= 2 && allEvaluations.Count > 0 &&
                            unit.ToUpper() != "M" && j == 0)
                            occurrences.Remove(occurrences.First());

                        else if (occurrences.Count >= 2 && allEvaluations.Count > 0 && unit.ToUpper() == "M"
                                 && patientSchedule.VisitsPerUnit >= 2 && patientSchedule.NumberOfUnits == 1 && j > 0)
                            occurrences.Remove(occurrences.First());

                        foreach (var item in occurrences)
                        {
                            var date = new PatientDate
                            {
                                PatientId = patientSchedule.PatientId,
                                ClinicianId = patientSchedule.ClinicianId,
                                RecertId = patientSchedule.RecertId,
                                ScheduleId = patientSchedule1.Id,
                                PatientDates = item.Period.StartTime.Date,
                                Type = "RoutineVisit",
                                FrequencyType = "W"
                            };
                            if (item.Period.StartTime.Date <= eoc.Date)
                            {
                                Common.Instances.PatientProfileInst.PatientDateSave(date);
                                newdate = unit == "m"
                                    ? item.Period.StartTime.Date
                                    : item.Period.StartTime.Date.AddDays(1);
                                calendar = new Calendar();
                            }
                        }

                        //var disChargeDate = unit == "m"
                        //    ? Utility.GetLastDayOfMonth(newdate)
                        //    : Utility.GetNextWeekday(newdate, DayOfWeek.Saturday);

                        var disChargeDate = unit == "m"
                            ? Utility.GetLastDayOfMonth(newdate)
                            : newdate.AddDays(-1);


                        if (eoc.Year != 0001)
                        {
                            var status =
                                unit.ToLower() == "m" && patientSchedule.NumberOfUnits == 1 &&
                                patientSchedule.VisitsPerUnit == 1
                                    ? "HIDE"
                                    : "N/A";
                            Common.Instances.PatientProfileInst.SaveDischargeDate(patientSchedule.PatientId,
                                patientSchedule.RecertId,
                                patientSchedule.ClinicianId, disChargeDate >= eoc ? eoc : disChargeDate, "Discharge",
                                status, unit.ToUpper());
                            newdate = disChargeDate >= eoc
                                ? eoc
                                : Utility.GetNextWeekday(disChargeDate, DayOfWeek.Sunday);
                        }

                        j++;
                    }
                    else
                    {
                        if (patientSchedule.NumberOfUnits <= 0) continue;
                        for (var idx = 0; idx <= patientSchedule.NumberOfUnits; idx++)
                        {
                            var date = new PatientDate
                            {
                                PatientId = patientSchedule.PatientId,
                                ClinicianId = patientSchedule.ClinicianId,
                                RecertId = patientSchedule.RecertId,
                                ScheduleId = patientSchedule1.Id,
                                PatientDates = newdate.Year < 1970
                                    ? evaluation
                                    : Utility.GetNextWeekday(newdate, DayOfWeek.Sunday),
                                Type = "Hold",
                                FrequencyType = "W"
                            };

                            if (date.PatientDates <= eoc.Date)
                            {
                                Common.Instances.PatientProfileInst.PatientDateSave(date);
                                newdate = date.PatientDates.AddDays(1);
                                calendar = new Calendar();
                            }

                            idx++;
                            newdate = Utility.GetNextWeekday(newdate, DayOfWeek.Sunday);
                        }

                        j++;
                    }
                }
            }

            // FREQUENCY TYPE UPDATE FOR 30DRE FOR MONTHLY AND WEEKLY VISIT
            var Get30DRE = new PatientDateRepo().GetPatientDatesByType(patientSchedule.PatientId, clinicianId,
                patientSchedule.RecertId, "30DRE");

            if (Get30DRE.Id > 0)
            {
                var GetRvAfter30D = new PatientDateRepo().GetLastPatientDate(patientSchedule.PatientId, clinicianId,
                patientSchedule.RecertId).Where(x => x.Type == "RoutineVisit" && x.PatientDates >= Get30DRE.PatientDates).OrderBy(y => y.PatientDates).FirstOrDefault();
                if (GetRvAfter30D != null)
                {
                    Get30DRE.FrequencyType = GetRvAfter30D.FrequencyType;
                    new PatientDateRepo().Save(Get30DRE);
                }

                else if (GetRvAfter30D == null)
                {
                    var GetRvbefore30D = new PatientDateRepo().GetLastPatientDate(patientSchedule.PatientId, clinicianId,
                patientSchedule.RecertId).Where(x => x.Type == "RoutineVisit" && x.PatientDates <= Get30DRE.PatientDates).OrderBy(y => y.PatientDates).FirstOrDefault();
                    if (GetRvbefore30D != null)
                    {
                        Get30DRE.FrequencyType = GetRvbefore30D.FrequencyType;
                        new PatientDateRepo().Save(Get30DRE);
                    }
                }
            }
            // END PROCESS


            return newlist;
        }

        public Page<PatientSchedule> GetPatientSchedule(int pagenumber, int pagesize, int patientId = 0,
            int recertId = 0, int currentLoggedUser = 0, bool IsIncludeAllFrequency = false)
        {
            var cmd = Sql.Builder.Select(
                    $@"ps.*,Concat(CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150)),' ',
           CAST(AES_DECRYPT(UNHEX(pp.LastName),'{Encryption.Key}') AS CHAR (150))) as PatientName,
           Concat(u.FirstName,' ',u.LastName) as ClinicianName")
                .From("PatientSchedule ps");
            cmd.InnerJoin("PatientProfiles pp").On("ps.PatientId = pp.Id");
            cmd.InnerJoin("Users u").On("u.Id = ps.ClinicianId");

            if (!IsIncludeAllFrequency)
                cmd.Where("ps.ClinicianId= @0 ", currentLoggedUser);

            cmd.Where("ps.PatientId=@0 AND ps.RecertId=@1", patientId, recertId);
            using (var db = Utility.Database)
            {
                var result = db.Page<PatientSchedule>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public int DeleteByPatientId(int patientId, int recertId, int clinicianId)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from PatientSchedule");
                cmd.Where("PatientId=@0 AND RecertId=@1 AND ClinicianId=@2 ", patientId, recertId, clinicianId);
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                const string cmd = @"Delete FROM PatientSchedule where Id=@0;
                                     Delete FROM PatientDates where ScheduleId=@0";
                var result = db.Execute(cmd, Id);
                return Convert.ToInt32(result);
            }
        }

        public int DeletePatientDatesByPatientid(int patientid, int recertId, string type = "RoutineVisit",
            int clinicianId = 0)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from PatientDates");
                cmd.Where("PatientId=@0 AND RecertId=@1 AND ClinicianId=@2", patientid, recertId, clinicianId);
                cmd.Where("Type=@0 OR Type='Hold' OR Type='Discharge'", type);
                var result = db.Execute(cmd);
                Update30DRE(patientid, recertId, "30DRE", clinicianId);
                return Convert.ToInt32(result);
            }
        }

        public int UpdateEval(int patientid, int recertId, int clinicianId = 0, string Freqtype = "N/A")
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append($@"UPDATE PatientDates SET FrequencyType = '{Freqtype}'");
                cmd.Where("PatientId=@0 AND RecertId=@1 AND ClinicianId=@2 AND Type='Evaluation'", patientid, recertId,
                    clinicianId);
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }

        public int Update30DRE(int patientid, int recertId, string type = "30DRE", int clinicianId = 0)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("UPDATE PatientDates SET IsAddedToSchedule = false, Status = 'N/A'");
                cmd.Where("PatientId=@0 AND RecertId=@1 AND ClinicianId=@2", patientid, recertId, clinicianId);
                cmd.Where("Type=@0 OR Type='Evaluation'", type);
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }
    }
}