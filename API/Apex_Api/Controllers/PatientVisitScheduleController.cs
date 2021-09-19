using AgileObjects.AgileMapper.Extensions;
using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using Apex.DataAccess.Response;
using Apex_Api.Service;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class PatientVisitScheduleController : ControllerBase
    {
        [HttpGet("GetVisitSchedule")]
        public ApiResponse GetVisitSchedule(DateTime startdate, int pagenumber = 1, int pagesize = 20,
            string query = "", string mode = "Manual", int clinicianId = 0, bool saveSchedule = true)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                var splitTimeHalf = 0;
                DateTime newSplitstartTime = new DateTime();

                var referer = Request.Headers["X-Referer"].ToString();
                var permissionName = mode == "Manual" ? "MANUAL" : "SEMIAUTO";
                var getPermission = Common.Instances.PlanPermissionInst.GetPlanPermissionByPlanName(me.PlanName,
                    $"SCHEDULING.{permissionName}.{referer.ToUpper()}");

                if (getPermission == null && referer != "APP")
                {
                    throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized, Constant.NOPERMISSION);
                }

                int sessionStart;

                var isManualMode = mode.ToLower() == "manual";

                double userlatitude;
                double userlongitude;

                var newEndTime = new DateTime();

                var j = 0;

                //list
                var patientList = new List<PatientVisitScheduleGet>();

                // We need to add clinicianId, can only be consumed if admin role
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                var mySettings = Instances.SettingInst.Get(me.Id);
                var agencyCoordinates = Instances.ApiInst.GetAgency(me.AgencyId);

                userlatitude = agencyCoordinates.Id > 0 && mySettings.DistanceCalculator == Constant.NURSEADDRESS
                    ? agencyCoordinates.Latitude
                    : me.Lat;

                userlongitude = agencyCoordinates.Id > 0 && mySettings.DistanceCalculator == Constant.NURSEADDRESS
                    ? agencyCoordinates.Longitude
                    : me.Long;

                var data = Common.Instances.VisitScheduleRepoInst.GetVisitScheduleByDate(
                    startdate.Date.ToString("yyyy-MM-dd"),
                    pagenumber,
                    pagesize, query, me.Id);

                foreach (var item in data.Items)
                {
                    var getPatientVisit = new PatientVisitScheduleGet();
                    getPatientVisit = item.Map().Over(getPatientVisit);

                    patientList.Add(getPatientVisit);
                }

                if (isManualMode)
                {
                    new PatientVisitScheduleService().LoadDistanceMatrix(patientList, userlatitude, userlongitude);
                }
                else
                {
                    if (data.Items.Any(x => x.IsLocked))
                    {
                        res.Message = "Please unlock all visits to use semi-auto or auto mode";
                        res.Status = 400;
                        return res;
                    }

                    //SORT BASED ON START POSITION
                    new PatientVisitScheduleService().SortByDistance(patientList, userlatitude, userlongitude);
                    
                    //LOAD DISTANCE DATA BASED ON NEW ORDER
                    new PatientVisitScheduleService().LoadDistanceMatrix(patientList, userlatitude, userlongitude);
                    
                    foreach (var item in patientList)
                    {
                        var nurseSetting = Instances.SettingsRepoInst.Get(item.NurseId);

                        var newStartTime = item.Start.Date;
                        var sessionTime = Utility.GetSessionTime(item.colorType, nurseSetting);
                        sessionStart = mySettings?.Start.Hours ?? 8;
                        if (j == 0)
                            item.Start = newStartTime.AddHours(sessionStart);
                        else
                        {
                            item.Start = newEndTime.AddMinutes(item.DurationMins);
                            //prevent visit overlap
                            if (item.Start < patientList[j - 1].End) item.Start = patientList[j - 1].End;
                        }

                        item.End = item.Start.AddMinutes(sessionTime);
                        newEndTime = item.End;



                        //SAVE WHEN SEMI AUTO MODE AND BOOL TRUE
                        if (saveSchedule)
                        {
                            var saveItem = data.Items.FirstOrDefault((x => x.Id == item.Id));
                            if (saveItem == null) continue;
                            saveItem = item.Map().Over(saveItem);

                            // LOGIC 50% split logic on semi auto HALF SPLIT VISIT TIME                         
                            if (!string.IsNullOrWhiteSpace(saveItem.CombinationVisit)  && saveItem.CombinationVisit.Length >=32)
                            {
                                TimeSpan value = saveItem.End.Subtract(saveItem.Start);
                                int minutes = (int)value.TotalMinutes;

                                saveItem.Start = splitTimeHalf == 0 ? saveItem.Start : newSplitstartTime;
                                saveItem.End =  saveItem.End.AddMinutes(- (splitTimeHalf == 0 ? (minutes /2): minutes));
                                
                                newSplitstartTime = saveItem.End;
                                splitTimeHalf++;
                            }
                            Instances.VisitScheduleRepoInst.Save(saveItem);
                        }
                        j++;
                    }
                }


                var srv = new DriveHistoryService();
                Instances.DriveHistoryInst.DeleteMultiple(patientList);

                var result = patientList;
                srv.SaveDriveHistoryMultiple(result, me.Id);
                new PatientVisitScheduleService().CombineVisits(ref result);

                res.Data = result;
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpGet("GetSinglePatientVisitSchedule")]
        public ApiResponse GetSinglePatientVisitSchedule([Required] int Patientid, int pagenumber = 1,
            int pagesize = 20, string query = "", int clinicianId = 0)
        {
            var res = new ApiResponse();

            var me = GetUserbyToken(HttpContext);
            try

            {
                // We need to add clinicianId, can only be consumed if admin role
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                res.Events =
                    Common.Instances.VisitScheduleRepoInst.GetSingleVisitSchedule(me.Id, Patientid, pagenumber,
                        pagesize,
                        query);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpPost("AddToVisitSchedule")]
        public ApiResponse AddToVisitSchedule([FromBody] PatientVisitScheduleSave visitSchedule, int clinicianId = 0
            , string mode = Constant.TODAY)
        {
            var res = new ApiResponse();
            try
            {
                var me = GetUserbyToken(HttpContext);

                // Only be consumed if admin role
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                if (me.RoleName.ToUpper().Trim() == Constant.ADMIN)
                {
                    var getUser = Common.Instances.User.Get(me.Id);
                    me.IncludeWeekendsInWeekView = getUser.IncludeWeekendsInWeekView;
                }

                var getTodayDate = visitSchedule.VisitDate;
                var weekDay = getTodayDate.DayOfWeek.ToString();

                if (!me.IncludeWeekendsInWeekViewEnabled && weekDay.IsWeekend())
                    throw new HttpException(res.Status = (int) HttpStatusCode.Unauthorized,
                        Constant.IncludeWeekendsInWeekView);

                foreach (var item in visitSchedule.Patients)
                {
                    if (item.CertEndDate.Year == 0001) continue;

                    var patient = Common.Instances.PatientProfileInst.Get(item.PatientId, me.Id);
                    if (visitSchedule.VisitDate > item.CertEndDate)
                        throw new HttpException(res.Status = (int) HttpStatusCode.Unauthorized,
                            $"Patient {patient.FullName} visit cannot be schedule after EOC.");

                    if (item.IsRecert && !item.IsRecertWithinRange(visitSchedule.VisitDate)) //continue;              
                        throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized,
                        $"Patient {patient.FullName} Recert visit schedule between only {item.CertEndDate.AddDays(-5).ToString("MMM dd, yyyy")} - {item.CertEndDate.AddDays(1).ToString("MMM dd, yyyy")}");                                                 
                }

                //res.Data = new PatientVisitScheduleService().AddToVisitSchedule(visitSchedule,
                //    ref res, me.Id, mode.ToUpper().Trim(),
                //    me.IncludeWeekendsInWeekViewEnabled);


                new PatientVisitScheduleService().AddToVisitSchedule(visitSchedule, ref res, me.Id, mode.ToUpper().Trim(),me.IncludeWeekendsInWeekViewEnabled);
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = 402;
            }

            return res;
        }

        // Delete by visitSchedule primary key
        [HttpDelete("RemoveFromVisitSchedule")]
        public ApiResponse RemoveFromVisitSchedule([Required] int id, int clinicianId = 0)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                // Only be consumed if admin role
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                var visits = new List<PatientVisitSchedule>();
                var set = new HashSet<int>();
                var uniquesVisit = visits.Where(x => set.Add(x.Id));

                var visitSchedule = Common.Instances.VisitScheduleRepoInst.GetVisitSchedule(id);
                visits.Add(visitSchedule);

                if (visitSchedule.Id > 0)
                {
                    if (!string.IsNullOrWhiteSpace(visitSchedule.CombinationVisit) && visitSchedule.CombinationVisit.Length >= 32)
                    {
                        var combinationvisit = Common.Instances.VisitScheduleRepoInst.GetCombinationVisit(visitSchedule.CombinationVisit);
                        visits.AddRange(combinationvisit);
                    }

                    set = new HashSet<int>();
                    uniquesVisit = visits.Where(x => set.Add(x.Id));

                    foreach (var item in uniquesVisit)
                    {
                        var type = Utility.GetScheduleType(item.colorType);
                        var start = item.RoutineVisitDate.ToString("yyyy-MM-dd");
                        UpdateIsAddedToScheduleFalse(item.PatientId, type, start, me.Id);
                        Common.Instances.VisitScheduleRepoInst.Delete(item.Id);
                    }       
                }
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }


        // VISIT DRAG DROP
        [HttpGet("UpdatePatientVisitSchedule")]
        public ApiResponse UpdatePatientVisitSchedule([Required] int visitScheduleId, [Required] DateTime StartDate,
             [Required] DateTime EndDate, int clinicianId = 0, bool isCombined = false, string combinationVisit = "")
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                // We need to add clinicianId, can only be consumed if admin role
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                var visitSchedule = Common.Instances.VisitScheduleRepoInst.GetVisitSchedule(visitScheduleId);
                if (visitSchedule.IsLocked)
                {
                    res.Message = "Please unlock to move";
                    res.Status = 400;
                    return res;
                }

                if (visitSchedule.Id == 0) throw new HttpException(404, "Patient Visit Not Found");

                if (me.RoleName.ToUpper().Trim() == Constant.ADMIN)
                {
                    var getUser = Common.Instances.User.Get(me.Id);
                    me.IncludeWeekendsInWeekView = getUser.IncludeWeekendsInWeekView;
                }

                var weekDay = StartDate.DayOfWeek.ToString();
                if (me.IncludeWeekendsInWeekView.ToUpper().Trim() == "NO" &&
                    (weekDay == Constant.SATURDAY || weekDay == Constant.SUNDAY))
                    throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized,
                        Constant.IncludeWeekendsInWeekView);

                if (string.IsNullOrWhiteSpace(combinationVisit))
                {
                    var checkpatientExist =
                        Common.Instances.VisitScheduleInst.CheckPatientSchdeuleExist(me.Id, visitSchedule.PatientId,
                            StartDate.Date);
                    if (checkpatientExist != null && checkpatientExist.Start.Date == StartDate.Date &&
                        checkpatientExist.Id != visitScheduleId)
                        throw new HttpException(402,
                            $"Patient {checkpatientExist.PatientName} already added for {checkpatientExist.Start.Date.ToString("MMMM dd yyyy")}. Please add the patient for another day.");
                }

                var combinationList = new List<object>();
                var result = new object();

                if (!string.IsNullOrWhiteSpace(combinationVisit) && combinationVisit.Length >= 32)
                {
                    var combinationvisit = Common.Instances.VisitScheduleRepoInst.GetCombinationVisit(combinationVisit);
                    var i = 0;
                    DateTime dateTime = new DateTime();

                    foreach (var item in combinationvisit)
                    {
                        TimeSpan value = item.End.Subtract(item.Start);
                        int minutes = (int)value.TotalMinutes;

                        item.Start = i == 0 ? StartDate : dateTime;
                        item.End = item.Start.AddMinutes(minutes);

                        bool isvisitSlotavailable = SlotVisitAvailable(item.Start, item.End.AddMinutes(i == 0 ? minutes : 0), me.Id);
                        if (isvisitSlotavailable)
                        {
                            combinationList.Add(Instances.VisitScheduleRepoInst.Save(item));
                            i++;
                            dateTime = item.End;
                        }

                        else
                        {
                            throw new HttpException(402,
                              $"Visit slot not available please drag another time and date.");
                        }
                    }
                }
                else
                {
                    visitSchedule.Start = StartDate;
                    visitSchedule.End = EndDate;
                    result = Instances.VisitScheduleRepoInst.Save(visitSchedule);
                }
                res.Data = combinationList.Count > 0 ? combinationList : result;
                res.Message = Constant.Message;
                res.Status = 200;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = 402;
            }

            return res;
        }

        [HttpPost("UpdateIsAddedToScheduleFalse")]
        public int UpdateIsAddedToScheduleFalse(int patientId, string type, string start, int ClinicianId)
        {
            using (var db = Utility.Database)
            {
                var result = 0;
                if (type == "RoutineVisit")
                {
                    var cmd = Sql.Builder.Append(
                        "UPDATE PatientDates SET IsAddedToSchedule=false, Status = 'N/A' WHERE PatientId =@0 AND Type=@1 AND cast(PatientDates as date) =@2",
                        patientId, type, start);
                    result = db.Execute(cmd);
                }
                else
                {
                    var cmd = Sql.Builder.Append(
                        "UPDATE PatientDates SET IsAddedToSchedule=false , Status = 'N/A' WHERE PatientId =@0 AND Type=@1 AND ClinicianId=@2",
                        patientId,
                        type, ClinicianId);
                    result = db.Execute(cmd);
                }

                return Convert.ToInt32(result);
            }
        }


        // UPDATE VISIT STATUS MARK MISSED / MARK COMPLETE
        [HttpPost("UpdateVisitStatus")]
        public ApiResponse UpdateVisitStatus(int id, string status = "None", int patientDateId = 0, int clinicianId = 0)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                // We need to add clinicianId, can only be consumed if admin role
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                var visits = new List<PatientVisitSchedule>();
                var set = new HashSet<int>();
                var uniquesVisit = visits.Where(x => set.Add(x.Id));
              

                if (id <= 0 && patientDateId > 0 && status.ToUpper().Trim() != Constant.VISITSTATUSCOMPLETED)
                {
                    var GetPatientDate = new PatientDateRepo().Get(patientDateId);
                    if (GetPatientDate.Id > 0)
                    {
                        GetPatientDate.Status = status.ToUpper().Trim();
                        res.Data = new PatientDateRepo().Save(GetPatientDate);
                    }
                }

                // MISSED
                else if (id > 0 && patientDateId <= 0 && status.ToUpper().Trim() != Constant.VISITSTATUSCOMPLETED)
                {
                    visits = new List<PatientVisitSchedule>();
                    var getvisitSchedule = Common.Instances.VisitScheduleRepoInst.GetVisitSchedule(id);
                    visits.Add(getvisitSchedule);

                    if (getvisitSchedule.Id > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(getvisitSchedule.CombinationVisit) && getvisitSchedule.CombinationVisit.Length >= 32)
                        {
                            var combinationvisit = Common.Instances.VisitScheduleRepoInst.GetCombinationVisit(getvisitSchedule.CombinationVisit);
                            visits.AddRange(combinationvisit);
                        }
              
                        set = new HashSet<int>(); 
                        uniquesVisit = visits.Where(x => set.Add(x.Id));

                        foreach (var item in uniquesVisit)
                        {
                            string Type = Utility.GetScheduleType(item.colorType);

                            var GetPatientDate = Common.Instances.VisitScheduleRepoInst.GetPatientDate(
                                item.RoutineVisitDate.ToString("yyyy-MM-dd")
                                , item.PatientId, Type, me.Id);
                            RemoveFromVisitSchedule(item.Id);
                            GetPatientDate.Status = status.ToUpper().Trim();
                            GetPatientDate.IsAddedToSchedule = false;
                            res.Data = Common.Instances.PatientProfileInst.PatientDateSave(GetPatientDate);
                        }                     
                    }
                }

                // MARK COMPLETE
                else if (id > 0 && patientDateId <= 0 && status.ToUpper().Trim() == Constant.VISITSTATUSCOMPLETED)
                {
                    visits = new List<PatientVisitSchedule>();
                    var getvisitSchedule = Common.Instances.VisitScheduleRepoInst.GetVisitSchedule(id);
                    visits.Add(getvisitSchedule);
                    var patient = Common.Instances.PatientProfile.Get(getvisitSchedule.PatientId, me.Id);
                    if (getvisitSchedule.Id > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(getvisitSchedule.CombinationVisit) && getvisitSchedule.CombinationVisit.Length >= 32)
                        {
                            var combinationvisit = Common.Instances.VisitScheduleRepoInst.GetCombinationVisit(getvisitSchedule.CombinationVisit);
                            visits.AddRange(combinationvisit);
                        }

                        set = new HashSet<int>();
                        uniquesVisit = visits.Where(x => set.Add(x.Id));
                        foreach (var item in uniquesVisit)
                        {
                            string Type = Utility.GetScheduleType(item.colorType);
                            var GetPatientDate = Common.Instances.VisitScheduleRepoInst.GetPatientDate(
                                item.RoutineVisitDate.ToString("yyyy-MM-dd")
                                , item.PatientId, Type, me.Id);
                            GetPatientDate.Status = status.ToUpper().Trim();
                            GetPatientDate.PatientDates = Type == "Evaluation" ? item.Start :
                                Type == "30DRE" ? item.Start : item.RoutineVisitDate.Date;
                            res.Data = Common.Instances.PatientProfileInst.PatientDateSave(GetPatientDate);
                        }
                        
                    }
                }

                res.Message = res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = 402;
            }

            return res;
        }

        [HttpGet("SetVisitLockStatus")]
        public ApiResponse SetVisitLockStatus([Required] int visitScheduleId, bool isLocked = false)
        {
            var res = new ApiResponse();
            try
            {
                var vs = Instances.VisitScheduleRepoInst.GetVisitSchedule(visitScheduleId);
                if (vs == null || vs.Id <= 0)
                {
                    res.Status = 404;
                    res.Message = "Visit not found";
                    return res;
                }

                vs.IsLocked = isLocked;
                Instances.VisitScheduleRepoInst.Save(vs);
                res.Data = vs;
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = 500;
            }

            return res;
        }

        [HttpPost("AutoScheduling")]
        public ApiResponse AutoScheduling([Required] DateTime startdate, [Required] string Mode = Constant.TODAY,
            int clinicianId = 0)
        {
            var res = new ApiResponse();
            try
            {
                var ClinicianId = new List<int>();
                var me = Common.GetUserbyToken(HttpContext);

                // We need to add clinicianId, can only be consumed if admin role
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;
                ClinicianId.Add(me.Id);
                //var referer = Request.Headers["X-Referer"].ToString();
                //var getPermission =
                //    Common.Instances.PlanPermissionInst.GetPlanPermissionByPlanName(me.PlanName,
                //        $"SCHEDULING.AUTO.{referer.ToUpper()}");
                //;
                //if (getPermission == null)
                //{
                //    throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized, Constant.NOPERMISSION);
                //}

                if (me.RoleName == "OTA" || me.RoleName == "PTA" || me.RoleName == "AID")
                {
                    if (me.RoleName == "OTA")
                        ClinicianId.AddRange(Common.Instances.PatientProfile
                            .GetAllPatient(ClinicianId, me.RoleName, "", "").Where(x => x.OT != 0).Select(x => x.OT)
                            .ToList());
                    else if (me.RoleName == "PTA")
                        ClinicianId.AddRange(Common.Instances.PatientProfile
                            .GetAllPatient(ClinicianId, "", me.RoleName, "").Where(x => x.PT != 0).Select(x => x.PT)
                            .ToList());
                    else if (me.RoleName == "AID")
                    {
                        var ids = Common.Instances.PatientProfile.GetAllPatient(ClinicianId, "", "", me.RoleName)
                            .Where(x => x.OT != 0 && x.PT != 0 && x.SN != 0 && x.SLP != 0)
                            .Select(x => new { x.OT, x.PT, x.SN, x.SLP }).ToArray();
                        foreach (var item in ids)
                        {
                            ClinicianId.Add(item.OT);
                            ClinicianId.Add(item.PT);
                            ClinicianId.Add(item.SN);
                            ClinicianId.Add(item.SLP);
                        }
                    }
                }

                if (me.RoleName.ToUpper().Trim() == "ADMIN")
                {
                    var getUser = Common.Instances.User.Get(me.Id);
                    me.IncludeWeekendsInWeekView = getUser.IncludeWeekendsInWeekView;
                }

                DateTime getTodayDate = startdate;
                string WeekDay = getTodayDate.DayOfWeek.ToString();
                if (me.IncludeWeekendsInWeekView.ToUpper().Trim() == "NO" &&
                    (WeekDay == Constant.SATURDAY || WeekDay == Constant.SUNDAY))
                {
                    throw new HttpException(res.Status = 402,
                        Constant.IncludeWeekendsInWeekView);
                }

                int sortidx = 0;
                var combine = 0;

                var patientList = new List<Patient>();

                bool isSunday = getTodayDate.DayOfWeek == 0;

                var dayOfweek = isSunday == false ? (int)getTodayDate.DayOfWeek : 7;

                DateTime startOfWeek = isSunday == true ? getTodayDate : getTodayDate.AddDays(-dayOfweek);

                var GetScheduledata = new PatientProfileService().GetSchedule(new List<int>(), ClinicianId, startOfWeek, "",
                    true, "Active", me.AgencyId, me.RoleName, startdate.ToString("yyyy-MM-dd"), true).ToList();

                var daysLeft = 0;

                if (Mode == Constant.TODAY)
                    daysLeft = 0;

                else if (Mode.ToUpper().Trim() == "WEEK")
                    daysLeft = Utility.GetLeftDaysInWeek(startdate) - 1 < 0
                        ? 0
                        : Utility.GetLeftDaysInWeek(startdate) - 1;


                else if (Mode.ToUpper().Trim() == "MONTH")
                    daysLeft = Utility.GetRemaningDaysInMonth(startdate) - 1;


                for (int idx = 0; idx <= daysLeft; idx++)
                {
                    var patient = new Patient();
                    foreach (var item in GetScheduledata)
                    {
                        {
                            //patient.PatientId = item.PatientId;
                            var exists = item.vsttype.ElementAtOrDefault(idx) != null;
                            if (exists)
                            {
                                bool CombineVisit = item.vsttype[0].IsCombined == true ? true : false;
                                
                                if (!CombineVisit)
                                {
                                    patient = new Patient();
                                    patient.PatientId = item.PatientId;
                                    patient.ClinicianId = item.vsttype[idx].ClinicianId;
                                    patient.colorType = item.vsttype[idx].VisitTypeCode;
                                    patient.RoutineVisitDate = item.vsttype[idx].RoutineVisitDate;
                                    patient.IsDisabled = item.vsttype[idx].IsDisabled;
                                    patient.IsCombined = item.vsttype[idx].IsCombined;
                                    patient.CertEndDate = item.vsttype[idx].CertEndDate;
                                    patient.RecertId = item.vsttype[idx].RecertId;
                                    patient.SortIndex = sortidx++;
                                    patientList.Add(patient);
                                }
                               
                                else
                                {
                                    for (int i = 0; i < item.vsttype.Count; i++)
                                    {
                                        patient = new Patient();
                                        patient.PatientId = item.PatientId;
                                        patient.ClinicianId = item.vsttype[i].ClinicianId;
                                        patient.colorType = item.vsttype[i].VisitTypeCode;
                                        patient.RoutineVisitDate = item.vsttype[i].RoutineVisitDate;
                                        patient.IsDisabled = item.vsttype[i].IsDisabled;
                                        patient.IsCombined = item.vsttype[i].IsCombined;
                                        patient.CertEndDate = item.vsttype[i].CertEndDate;
                                        patient.RecertId = item.vsttype[i].RecertId;
                                        patient.SortIndex = sortidx++;
                                        patientList.Add(patient);
                                    }
                                }
                            }
                        }
                    }

                    bool iscombined = patientList.Any(x => x.IsCombined == true);

                    var savePatientSchedule = new PatientVisitScheduleSave
                    {
                        NurseId = me.Id,
                        VisitDate = !iscombined
                            ? getTodayDate.AddDays(idx)
                            : getTodayDate.AddDays(combine == 1 ? idx - 1 : idx),
                        AddedBy = me.Id,
                        
                        Patients = patientList.ToList(),
                    };
                    if (savePatientSchedule.Patients.Count > 0)
                    {
                        var result = AddToVisitSchedule(savePatientSchedule, me.Id, Mode.ToUpper().Trim());
                        if (result.Status == 402)
                        {
                            res.Message = result.Message;
                            res.Status = result.Status;
                            return res;
                        }

                        res.Data = result;
                        res.Message = Constant.Message;
                        res.Status = 200;
                    }

                    patientList = new List<Patient>();
                    combine = iscombined ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = 402;
            }

            return res;
        }

        private bool SlotVisitAvailable(DateTime StartDate, DateTime EndDate, int meId)
        {
            var data = Common.Instances.VisitScheduleRepoInst.GetVisitScheduleByDate(
                   StartDate.Date.ToString("yyyy-MM-dd"), 1, 10000, "", meId);
            return !data.Items.Any(visit =>
                           //visit.Start >= StartDate && visit.End <= EndDate

                           StartDate < visit.End && visit.Start < EndDate
                    );
        }
        
    }
}