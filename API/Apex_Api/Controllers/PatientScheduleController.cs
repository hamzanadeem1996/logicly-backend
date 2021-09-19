using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class PatientScheduleController : ControllerBase
    {
        [HttpGet("Get")]
        public ApiResponse Get(int pagenumber = 1, int pagesize = 20, int patientId = 0, int recertId = 0, int clinicianId = 0)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                bool IsIncludeAllFrequency = me.RoleName == Constant.ADMIN && clinicianId == 0 ? true : false;

                res.Data = Common.Instances.patientProfileService.GetPatientSchedule(pagenumber, pagesize, patientId, recertId, me.Id, IsIncludeAllFrequency);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost("PatientScheduleSave")]
        public ApiResponse PatientScheduleSave([FromBody] PatientScheduleSave patientScheduleSave, int clinicianId = 0)
        {
            var res = new ApiResponse();
            var me = Common.GetUserbyToken(HttpContext);
            try
            {
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;
                res.Data = Common.Instances.patientProfileService.PatientScheduleSave(patientScheduleSave,
                    me.Id, null, me.RoleName);
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

        [HttpDelete("DeletePatientSchedule")]
        public ApiResponse DeletePatientSchedule([Required] int id)
        {
            var res = new ApiResponse();
            try
            {
                int status = 0;
                if (id == 0) { return res; }

                status = Common.Instances.patientProfileService.Delete(id);
                res.Message = status >= 1 ? Constant.Message : Constant.NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }
    }
}