using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using Apex.DataAccess.Response;
using Apex_Api.Service;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Apex_Api.Common;
using static Apex_Api.Service.GeoCodingService;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class PatientProfileController : ControllerBase
    {
        private readonly IHostingEnvironment _environment;
        private IConfiguration _Configuration;

        public PatientProfileController(IConfiguration Configuration, IHostingEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _Configuration = Configuration;
        }

        #region

        [HttpGet("Get")]
        public ApiResponse Get(int id)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                var patientProfile = Common.Instances.PatientProfile.Get(id, me.Id);

                //DateTime discharge = patientProfile.Discharge;

                if (patientProfile.Frequency == "N/A")
                    patientProfile.Discharge = DateTime.MinValue;

                //if (patientProfile.MultipleFrequency == "N/A")
                //    patientProfile.Discharge = DateTime.MinValue;
                //else
                //    patientProfile.Discharge = discharge;

                var result = patientProfile;
                FixNull(ref result);
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

        [HttpGet("GetAll")]
        public ApiResponse GetAll([FromQuery] List<int> Patientids, int pagenumber = 1, int pagesize = 20,
            string query = "", string Status = "", string filter = "ValidAddress")
        {
            var res = new ApiResponse();
            try
            {
                var patientList = new List<PatientProfile>();
                var ClinicianId = new List<int>();
                var me = GetUserbyToken(HttpContext);
                bool isAdmin = me.RoleName == Constant.ADMIN ? true : false;
                ClinicianId.Add(me.Id);

                var data = Common.Instances.PatientProfile.GetAll(Patientids, ClinicianId, pagenumber, pagesize, query,
                    Status,
                    filter.ToLower().Trim(), me.AgencyId, isAdmin);

                // fix Discharge date showing on table but on detail screen showing N/A
                foreach (var patientProfile in data.Items)
                {
                    //DateTime discharge = patientProfile.Discharge;

                    if (patientProfile.Frequency == "N/A")
                        patientProfile.Discharge = DateTime.MinValue;

                    //if (patientProfile.MultipleFrequency == "N/A")
                    //    patientProfile.Discharge = DateTime.MinValue;

                    //else
                    //    patientProfile.Discharge = discharge;

                    patientList.Add(patientProfile);
                }

                data.Items = null;
                data.Items = patientList;
                res.Data = data;
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = 402;
            }

            return res;
        }

        [HttpGet("GetCliniciansAssignedToPatient")]
        public ApiResponse GetCliniciansAssignedToPatient([Required] int patientId = 0, string query = "")
        {
            var res = new ApiResponse();
            try
            {
                var me = GetUserbyToken(HttpContext);
                var GetPatient = Common.Instances.PatientProfile.Get(patientId, me.Id);
                if (GetPatient != null)
                {
                    var Assignedids = new List<int>
                    {
                        //GetPatient.OT, GetPatient.OTA, GetPatient.PT, GetPatient.PTA,
                        //GetPatient.SLP, GetPatient.SN, GetPatient.AID,GetPatient.MSW,
                        //GetPatient.AddedBy
                        GetPatient.OT, GetPatient.PT, GetPatient.SLP, GetPatient.SN, GetPatient.TeamLeader
                    };
                    res.Data = Common.Instances.User.GetAllUserByMultipleIds(Assignedids, query);
                }

                res.Message = res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [AllowAnonymous]
        [HttpPost("Save")]
        public ApiResponse Save(PatientProfile patientProfile)
        {
            var res = new ApiResponse();
            try
            {
                var me = GetUserbyToken(HttpContext);
                patientProfile.AgencyId = me.AgencyId;
                patientProfile.Status = patientProfile.Id == 0 ? "Active" : patientProfile.Status;
                patientProfile.AddedBy = me.Id;
                patientProfile.LastModBy = me.Id;
                res.Data = new PatientProfileService().Save(patientProfile, me);

                var ids = new List<int>
                {
                    patientProfile.OT,
                    patientProfile.OTA,
                    patientProfile.PT,
                    patientProfile.PTA,
                    patientProfile.SLP,
                    patientProfile.SN,
                    patientProfile.AID,
                    patientProfile.MSW
                };

                var userProfiles = Instances.User.GetAllUserByMultipleIds(ids);
                foreach (var userinfo in userProfiles)
                {
                    var emailBody = EmailService.PatientAssignNurse();
                    var bodyHtml = emailBody = emailBody.Replace("{Name}", $"{userinfo.FirstName} {userinfo.LastName}")
                        .Replace("{PatientName}",
                            $"{Encryption.Decrypt(patientProfile.FirstName)} {Encryption.Decrypt(patientProfile.LastName)}");
                    var wasSent = EmailService.SendEmail(userinfo.Email, "PATIENT ASSIGN TO NURSE", true, bodyHtml,
                        "", _Configuration);

                    var message = (wasSent) ? "Email sent Successfully!" : "error in sent email!";
                    res.Message = message;
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

        [HttpGet("GetSchedule")]
        public ApiResponse GetSchedule([Required] DateTime startdate, string query = "", bool filterByDate = true,
            int clinicianId = 0)
        {
            var actualDate = startdate;

            var ClinicianId = new List<int>();

            var res = new ApiResponse();
            var me = Common.GetUserbyToken(HttpContext);
            me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

            ClinicianId.Add(me.Id);

            //if (me.RoleName == Constant.ADMIN && clinicianId <= 0)
            //    return res;

            if (me.RoleName == Constant.ADMIN && clinicianId > 0)
                me.RoleName = Common.Instances.User.Get(me.Id).RoleName;

            if (me.RoleName == "OTA")
                ClinicianId.AddRange(Common.Instances.PatientProfile.GetAllPatient(ClinicianId, me.RoleName, "", "")
                    .Where(x => x.OT != 0).Select(x => x.OT).ToList());
            else if (me.RoleName == "PTA")
                ClinicianId.AddRange(Common.Instances.PatientProfile.GetAllPatient(ClinicianId, "", me.RoleName, "")
                    .Where(x => x.PT != 0).Select(x => x.PT).ToList());
            else if (me.RoleName == "AID" || me.RoleName == "MSW")
            {
                var ids = Common.Instances.PatientProfile.GetAllPatient(ClinicianId, "", "", me.RoleName)
                    .Where(x => x.OT != 0 && x.PT != 0 && x.SN != 0 && x.SLP != 0)
                    .Select(x => new {x.OT, x.PT, x.SN, x.SLP}).ToArray();
                foreach (var item in ids)
                {
                    ClinicianId.Add(item.OT);
                    ClinicianId.Add(item.PT);
                    ClinicianId.Add(item.SN);
                    ClinicianId.Add(item.SLP);
                }
            }

            try
            {
                var Int = new List<int>();
                // Week Start Sunday // DateTime startdate

                var isSunday = startdate.DayOfWeek == 0;
                var dayOfweek = isSunday == false ? (int) startdate.DayOfWeek : 7;
                var startOfWeek = isSunday ? startdate : startdate.AddDays(-dayOfweek);
                // End Week Start Code
                res.Events = new PatientProfileService().GetSchedule(Int, ClinicianId, startOfWeek, query,
                    filterByDate, "Active", me.AgencyId, me.RoleName, actualDate.ToString("yyyy-MM-dd"));

                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpGet("UpdatePatientStatus")]
        public ApiResponse UpdatePatientStatus(int patientId, string status = "Active")
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                var isDischarged = status.ToUpper().Contains("DISCHAR");
                var patient = Common.Instances.PatientProfile.Get(patientId, me.Id);
                if (patient.Id > 0)
                {
                    if (isDischarged)
                    {
                        patient.Discharge = DateTime.UtcNow;

                        var PatientVisitHistory = new PatientVisitHistory
                        {
                            PatientId = patientId,
                            DischargeDate = DateTime.UtcNow,
                            Status = status.ToUpper()
                        };
                        Common.Instances.patientVisitHistory.Save(PatientVisitHistory);
                    }

                    patient.Status = status.ToUpper();
                    Instances.PatientProfile.Save(patient, me.Id);
                }

                res.Data = Common.Instances.PatientProfile.Get(patientId, me.Id);
                res.Message = isDischarged ? Constant.PATIENTDISCHARGE :
                    patient.Id == 0 ? Utility.ResponseMessage.NotFound : "Patient Active Now.";
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpPost("ImportPatientCsv")]
        public async Task<ApiResponse> ImportPatientCsv(IFormFile file)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                var referer = Request.Headers["X-Referer"].ToString();
                var getPermission =
                    Common.Instances.PlanPermissionInst.GetPlanPermissionByPlanName(me.PlanName, $"BULK.UPLOAD");
                if (getPermission == null)
                {
                    throw new HttpException(res.Status = (int) HttpStatusCode.Unauthorized, Constant.NOPERMISSION);
                }

                var count = 0;
                var patientList = new ImportCSVService().ImportCSV(file);

                // getAllpatient
                var patientAllList = Common.Instances.PatientProfile.GetAllPatientforCsv();

                // Get those patient not in patient table.
                var exceptList = patientList.Except(patientAllList, new PatientProfileComparer()).ToList();

                var unexceptList = patientList.Intersect(patientAllList, new PatientProfileComparer()).ToList();

                foreach (var item in exceptList)
                {
                    var stAdmission = Convert.ToDateTime(item.InitialAdmission).ToString("MM/dd/yyyy hh:mm:ss tt");
                    var coordinates = new GeoCodingService().GeoCoordinate(item.Address).Result;
                    var patient = new PatientProfile
                    {
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Admission = DateTime.ParseExact(stAdmission, "dd/MM/yyyy hh:mm:ss tt",
                            CultureInfo.InvariantCulture),
                        Address = item.Address,
                        CityName = coordinates.CityName,
                        Lat = coordinates.Latitude,
                        Long = coordinates.Longitude,
                        Status = item.Status,
                        AddedBy = me.Id,
                        LastModBy = me.Id,
                        TeamLeader = me.Id,
                        AgencyId = me.AgencyId,
                        MDNumber = item.MDNumber,
                        PrimaryNumber = item.PrimaryNumber,
                        SecondaryNumber = item.SecondaryNumber
                    };
                    Common.Instances.patientProfileService.Save(patient, me);
                    count++;
                }

                res.Data = unexceptList;
                res.Message = $"{count} Record Successfully Uploaded.";
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpGet("SetEvaluationDate")]
        public ApiResponse SetEvaluationDate([Required] int patientId, [Required] DateTime Date)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                var patient = Common.Instances.PatientProfile.Get(patientId, me.Id);

                if (patient == null)
                    throw new HttpException(res.Status = 402,
                        "Patient not found.");


                if (patient.EvalCompleted)
                    throw new HttpException(res.Status = 402,
                        "This evaluation is completed. so we can't change such evaluation.");

                if (patient != null && Date < patient.Admission)
                    throw new HttpException(res.Status = 402,
                        "Evaluation date should be greater than admission date.");
                else if (patient != null && Date > patient.Eoc)
                    throw new HttpException(res.Status = 402,
                        "Evaluation date should be less than end of care date.");

                var status = Common.Instances.PatientProfile.UpdateEvaluationDate(me.Id, patientId,
                    patient.ActiveCertId,
                    Date, patient.Frequency);

                if (status >= 1 && patient.ActiveCertId > 0)
                {
                    var schedule = new PatientScheduleSave
                    {
                        PatientId = patientId,
                        RecertId = patient.ActiveCertId,
                        GeneratedVisitCode = patient.Frequency.Split(',')
                    };
                    var data = new PatientProfileService().PatientScheduleSave(schedule,
                        me.Id, null, me.RoleName);
                }

                res.Message = status >= 1 ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        /// <summary>
        /// #381- Add a check that a patient with entries in VisitSchedule can’t be deleted until those visits are deleted
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("Delete")]
        public ApiResponse Delete(int id)
        {
            var res = new ApiResponse();
            try
            {
                var Getpatient = Common.Instances.VisitScheduleRepoInst.Get(id);
                if (Getpatient != null)
                {
                    res.Message = Constant.PatientDelete;
                }
                else
                {
                    int status = 0;
                    status = Common.Instances.PatientProfile.Delete(id);
                    res.Message = status >= 1 ? Constant.Message : Constant.NotFound;
                }
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpDelete("DeleteMultiplePatients")]
        public ApiResponse DeleteMultiplePatients(int[] id)
        {
            var res = new ApiResponse();
            try
            {
                int status = 0;
                string message = "";
                string patientName = "";
                foreach (var patientid in id)
                {
                    var Getpatient = Common.Instances.VisitScheduleRepoInst.Get(patientid);
                    if (Getpatient != null)
                    {
                        patientName += $"{Getpatient.PatientName},";
                        message = $"{patientName} {Constant.PatientDelete}";
                    }
                    else
                    {
                        status = Common.Instances.PatientProfile.Delete(patientid);
                    }

                    res.Message = message != "" ? message : status >= 1 ? Constant.Message : Constant.NotFound;
                }
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpGet("GetMultipleVisit")]
        public ApiResponse GetMultipleVisit([Required] int patientId, int clinicianId = 0, [Required] int recertId = 0,
            [Required] string type = "")
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                clinicianId = me.RoleName == Constant.ADMIN && clinicianId == 0 ? clinicianId : me.Id;
                res.Data = new PatientDateRepo().GetMultipleVisits(patientId, clinicianId, recertId, type);
                res.Message = res.Data != null ? Constant.Message : "Data not found.";
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        #endregion

        internal class PatientProfileComparer : IEqualityComparer<PatientCsvResponse>
        {
            public bool Equals(PatientCsvResponse x, PatientCsvResponse y)
            {
                if (string.Equals(x.PrimaryNumber, y.PrimaryNumber, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode(PatientCsvResponse obj)
            {
                return obj.FirstName.GetHashCode();
            }
        }
    }
}