using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using NPoco;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Apex_Api.Service
{
    public class PatientVisitScheduleService
    {
        public object AddToVisitSchedule(PatientVisitScheduleSave visitSchedule, ref ApiResponse res, int userid = 0,
            string mode = Constant.TODAY, bool includeWeekend = true)
        {
            var msg = "";
            var i = 0;
            var combineCount = 0;


            int sessionstart;
            var list = new List<string>();
            var name = new string[0];

            var userSettings = Common.Instances.SettingInst.Get(userid);

            var lastScheduleByDate = Common.Instances.VisitScheduleInst
                .GetVisitScheduleByDate(visitSchedule.VisitDate.Date.ToString("yyyy-MM-dd"), 1, 1000, "", userid).Items
                .LastOrDefault();

            var starttime = new DateTime();
            var newtime = Convert.ToDateTime(starttime.ToString("HH:mm:ss"));
            var newlist = new List<PatientVisitSchedule>();

            var guid = Guid.NewGuid();

            foreach (var item in visitSchedule.Patients)
            {         
                userid = item.ClinicianId;
                var lasttime = Common.Instances.VisitScheduleInst.GetAll(1, 1000, "", userid).Items.LastOrDefault();

                var checkpatientExist =
                    Common.Instances.VisitScheduleInst.CheckPatientSchdeuleExist(userid, item.PatientId,
                        visitSchedule.VisitDate);

                // - Scheduling the Combined visit on a day where other visits already scheduled, need to fix this point
                if (checkpatientExist == null || combineCount == 1 || (visitSchedule.Patients.Count == 1 &&
                                                                       visitSchedule.Patients.Any(x =>
                                                                           x.colorType == "D" && x.IsCombined == true)))
                {
                    if (item.IsCombined)
                        combineCount++;

                    if (combineCount > 2)
                    {
                        combineCount = 1;
                        guid = Guid.NewGuid();
                    }

                    var visitScheduleResponse = new PatientVisitSchedule();

                    // start calculate session length//

                    var defaultSessionTime = item.colorType == "E" ? userSettings.EvaluationSessionLength : //Evaluation
                        item.colorType == "R" ? userSettings.RecertSessionLength : //Recert
                        item.colorType == "D" ? userSettings.DischargeSessionLength : //Discharge
                        item.colorType == "30" ? userSettings.ThirtyDayReEvalSessionLength : // 30DRE
                        item.colorType == "RV" ? userSettings.TreatmentSessionLength : // Routine Visit
                        30; //Default 30

                    //For combined/split visits, are we doubling up the time. If the slot is for 90mins,
                    //we need to have two 45 min slots in the 90 min block. if combination r,d or 30,4 we pick highest session lenth

                    var recertVisit =
                        visitSchedule.Patients.Any(x => x.colorType == "R" && x.PatientId == item.PatientId);
                    var thirtyDVisit =
                        visitSchedule.Patients.Any(x => x.colorType == "30" && x.PatientId == item.PatientId);
                    var disCharge =
                        visitSchedule.Patients.Any(x => x.colorType == "D" && x.PatientId == item.PatientId);

                    if (recertVisit && disCharge && item.IsCombined && !thirtyDVisit)
                        defaultSessionTime = ((Math.Max(userSettings.RecertSessionLength,
                            userSettings.DischargeSessionLength)) / 2);
                    else if (thirtyDVisit && disCharge && item.IsCombined && !recertVisit)
                        defaultSessionTime = ((Math.Max(userSettings.ThirtyDayReEvalSessionLength,
                            userSettings.DischargeSessionLength)) / 2);

                    // End calculate session length//

                    visitScheduleResponse.NurseId = userid;
                    visitScheduleResponse.RecertId = item.RecertId;

                    if (visitScheduleResponse.AddedBy == 0) visitScheduleResponse.AddedBy = visitSchedule.AddedBy;
                    visitScheduleResponse.LastModBy = userid;
                    if (i == 0)
                    {
                        if (lasttime != null)
                        {
                            sessionstart = userSettings != null ? userSettings.Start.Hours : 8;

                            visitScheduleResponse.Start = visitSchedule.VisitDate.Date != lasttime.End.Date
                                                          && lastScheduleByDate == null
                                ? visitSchedule.VisitDate.Date.AddHours(sessionstart)
                                : lastScheduleByDate.End;

                            visitScheduleResponse.SortIndex = ++lasttime.SortIndex;
                            i = visitScheduleResponse.SortIndex;
                            i++;
                        }
                        else
                        {
                            sessionstart = userSettings != null ? userSettings.Start.Hours : 8;
                            visitScheduleResponse.Start = visitSchedule.VisitDate.Date.AddHours(sessionstart);
                            visitScheduleResponse.SortIndex = 0;
                            i++;
                        }
                    }
                    else
                    {
                        //set visit schedule for working hours
                        if (userSettings.End >= newtime.TimeOfDay
                            && userSettings.End < newtime.AddMinutes(defaultSessionTime)
                                .AddMinutes(item.IsCombined ? 0 : 30).TimeOfDay
                            && mode != Constant.TODAY)
                        {
                            sessionstart = userSettings != null ? userSettings.Start.Hours : 8;
                            newtime = newtime.Date.AddHours(sessionstart);

                            newtime = newtime.AddDays(1).AddMinutes(item.IsCombined ? 0 : -30);
                        }
                        //end

                        // add 30 mint diff first and second patient time
                        //visitScheduleResponse.Start = newtime.AddMinutes(GetDefaultsessiontime);
                        visitScheduleResponse.Start = newtime.AddMinutes(item.IsCombined ? 0 : 30);
                        visitScheduleResponse.SortIndex = i;
                        i++;
                    }

                    visitScheduleResponse.AllDay = false;
                    visitScheduleResponse.End = visitScheduleResponse.Start.AddMinutes(defaultSessionTime);
                    visitScheduleResponse.colorType = item.colorType;

                    if (mode == Constant.TODAY && userSettings.End < visitScheduleResponse.End.TimeOfDay)
                    {
                        res.Data = newlist;
                        res.Events = newlist;
                        res.Message =
                            $"{newlist.Count} Patients scheduled successfully. Please schedule remaining patients for tomorrow or change working hours.";
                        res.Status = 402;
                        return res;
                    }

                    // logic checking for if includeweekend no patient not schedule in saturday on sunday
                    var getTodayDate = visitScheduleResponse.Start;

                    if (mode != Constant.TODAY)
                    {
                        var stopSchedule = mode.ToUpper().Trim() == "WEEK"
                            ? Utility.GetNextWeekday(visitSchedule.VisitDate, DayOfWeek.Sunday)
                            : mode.ToUpper().Trim() == "MONTH"
                                ? Utility.GetLastDayOfMonth(visitSchedule.VisitDate)
                                : visitSchedule.VisitDate;
                        if (visitScheduleResponse.Start.Date > stopSchedule)
                        {
                            break;
                        }
                    }

                    var WeekDay = getTodayDate.DayOfWeek.ToString();

                    if (!includeWeekend && WeekDay.IsWeekend())
                    {
                        if (mode == "MONTH")
                        {
                            visitScheduleResponse.Start = visitScheduleResponse.Start.AddDays(2);
                            visitScheduleResponse.End = visitScheduleResponse.End.AddDays(2);
                        }
                        else if (mode == "WEEK")
                        {
                            break;
                        }
                    }

                    //add a column SortIndex to database. and api

                    //visitScheduleResponse.SortIndex = item.SortIndex;

                    newtime = visitScheduleResponse.Start;

                    visitScheduleResponse.PatientId = item.PatientId;

                    visitScheduleResponse.RoutineVisitDate = item.RoutineVisitDate;

                    visitScheduleResponse.CombinationVisit = item.IsCombined ? guid.ToString() : "";



                    Common.Instances.VisitScheduleInst.Save(visitScheduleResponse);

                    // Start Update IsAddedToSchedule in patient dates table

                    var Type = Utility.GetScheduleType(item.colorType);

                    var start = item.RoutineVisitDate.ToString("yyyy-MM-dd");
                    UpdateIsAddedToSchedule(item.PatientId, Type, start, userid);
                    // End

                    newlist.Add(visitScheduleResponse);
                    newtime = visitScheduleResponse.End;
                }


                if (visitSchedule.Patients.Count == 1 &&
                    visitSchedule.Patients.Any(x => x.colorType == "D" && x.IsCombined == true))
                {
                }

                else if (checkpatientExist != null && combineCount < 2)
                {
                    list.Add(checkpatientExist.Title);
                }
            }

            if (list.Count > 0)
            {
                name = list.ToArray();
                msg = string.Join(" and ", name);
            }

            res.Data = newlist;
            res.Events = newlist;
            res.Message = msg != ""
                ? $"Patient {msg} already added for {visitSchedule.VisitDate.Date.ToString("MMMM dd yyyy")}. Please add the patient for another day"
                : Constant.Message;
            res.Status = msg != "" ? 402 : 200;
            return res;
        }

        public int UpdateIsAddedToSchedule(int patientId, string type, string start, int userid = 0)
        {
            using (var db = Utility.Database)
            {
                var result = 0;
                if (type == "RoutineVisit")
                {
                    var cmd = Sql.Builder.Append(
                        "UPDATE PatientDates SET IsAddedToSchedule=true WHERE PatientId =@0 AND Type=@1 AND cast(PatientDates as date) =@2 AND ClinicianId=@3",
                        patientId, type, start, userid);
                    result = db.Execute(cmd);
                }
                else
                {
                    var cmd = Sql.Builder.Append(
                        "UPDATE PatientDates SET IsAddedToSchedule=true WHERE PatientId =@0 AND Type=@1 AND ClinicianId=@2",
                        patientId, type, userid);
                    result = db.Execute(cmd);
                }

                return Convert.ToInt32(result);
            }
        }

        public void LoadDistanceMatrix(PatientVisitScheduleGet patientVisit, double startLat,
            double startLon, double endLat, double endLon)
        {
            var distanceMatrix = new GoogleMapsService().GetDistanceMatrix(startLat, startLon,
                endLat, endLon);
            patientVisit.DurationMins = distanceMatrix.DurationMins;
            patientVisit.Driven = distanceMatrix.Distance;
            patientVisit.Duration = distanceMatrix.DurationStr;
            patientVisit.Units = patientVisit.Units;
            patientVisit.Distance = patientVisit.DistanceValue.ToString(CultureInfo.InvariantCulture);
        }

        public void LoadDistanceMatrix(List<PatientVisitScheduleGet> patientList, double userLatitude,
            double userLongitude)
        {
            var j = 0;
            foreach (var item in patientList)
            {
                double startLat;
                double startLong;

                //START FROM HOME FOR FIRST PATIENT AND FROM PATIENT TO NEXT THEREON
                if (j == 0)
                {
                    startLat = userLatitude;
                    startLong = userLongitude;
                }
                else
                {
                    startLat = patientList[j - 1].PatientLat;
                    startLong = patientList[j - 1].PatientLong;
                }

                new PatientVisitScheduleService().LoadDistanceMatrix(item, startLat, startLong,
                    item.PatientLat,
                    item.PatientLong);
                j++;
            }
        }

        public void CombineVisits(ref List<PatientVisitScheduleGet> result)
        {
            var combineVisit = result.Any(x => x.colorType == "30" || x.colorType == "R");
            var isDischarge = result.Any(y => y.colorType == "D");
            if (!combineVisit || !isDischarge) return;

            var exists = result.ElementAtOrDefault(result.FindIndex(ind => ind.colorType == "30"
                                                                           || ind.colorType == "R")) != null;
            var existsDischarge = result.ElementAtOrDefault(result.FindIndex(ind => ind.colorType == "D")) !=
                                  null;
            if (!exists || !existsDischarge) return;

            result[result.FindIndex(ind => ind.colorType == "30" || ind.colorType == "R")].IsPrimary = true;
            result[result.FindIndex(ind => ind.colorType == "D")].IsCombined = true;
            result[result.FindIndex(ind => ind.colorType == "30" || ind.colorType == "R")].IsCombined =
                true;
        }

        public List<PatientVisitScheduleGet> SortByDistance(List<PatientVisitScheduleGet> patientList, double startLat,
            double startLon)
        {
            foreach (var visit in patientList)
            {
                new PatientVisitScheduleService().LoadDistanceMatrix(visit, startLat, startLon,
                    visit.PatientLat,
                    visit.PatientLong);
            }

            patientList = patientList.OrderBy(x => x.Driven).ToList();
            return patientList;
        }
    }
}