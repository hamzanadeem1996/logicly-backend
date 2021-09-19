using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class PatientVisitHistoryController : ControllerBase
    {
        #region

        [HttpGet("GetDischargePatients")]
        public ApiResponse GetDischargePatients(int pagenumber = 1, int pagesize = 20, int patientid = 0)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.patientVisitHistory.GetAll(pagenumber, pagesize, "", patientid);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet("GetVisitHistory")]
        public ApiResponse GetVisitHistory(int pagenumber = 1, int pagesize = 30, int patientid = 0, int clinicianId = 0)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;
                var data = Common.Instances.VisitScheduleRepoInst.GetAll(pagenumber, pagesize, "", 0, patientid, me.Id).Items.Where(x => x.Start < DateTime.UtcNow);
                res.Data = data;
                res.Events = data;
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet("GetVisitType")]
        public ApiResponse GetVisitType()
        {
            var res = new ApiResponse();
            try
            {
                var data = new VisitType().GetAll();
                res.Data = data;
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        #endregion
    }
}