using Apex.DataAccess;
using Apex.DataAccess.Response;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Cors.EnableCors("CorsPolicy")]
    public class DashboardController : ControllerBase
    {
        [HttpGet("AgencyDashboard")]
        public ApiResponse AgencyDashboard([Required] string Date)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                if (me.RoleName != Constant.ADMIN)
                    throw new HttpException((int)HttpStatusCode.Unauthorized,
                        Utility.ResponseMessage.Unauthorized);

                bool valid = Utility.ValidateDate(Date);
                if (valid)
                {
                    string dateformat = "MM-dd-yyyy";
                    var dateTime = DateTime.ParseExact(Date, dateformat, new CultureInfo("en-US"), DateTimeStyles.None);
                    res.Data = Common.Instances.DashboardRepoInst.GetAgencyDashboard(me.AgencyId, dateTime.ToString("yyyy-MM-dd"));
                }
                res.Message = valid == false ? "Please enter valid date like MM-dd-yyyy" : res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = valid == false ? StatusCodes.Status401Unauthorized : res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet("ClinicianDashboard")]
        public ApiResponse ClinicianDashboard()
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                if (me.RoleName == Constant.ADMIN)
                    throw new HttpException((int)HttpStatusCode.Unauthorized,
                        Utility.ResponseMessage.Unauthorized);

                res.Data = Common.Instances.DashboardRepoInst.GetClinicianDashboard(me.Id, me.AgencyId);

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

        // WE USED THIS API FOR BOTH DASHBOARD AGENCY DASHBOARD AND CLINICIAN DASHBOARD
        [HttpGet("DrivenHistory")]
        public ApiResponse DrivenHistory([Required] string Date, int clinician = 0)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                bool valid = Utility.ValidateDate(Date);
                if (valid)
                {
                    string dateformat = "MM-dd-yyyy";
                    var dateTime = DateTime.ParseExact(Date, dateformat, new CultureInfo("en-US"), DateTimeStyles.None);
                    res.Data = Common.Instances.DashboardRepoInst.GetDrivenHistory(me.AgencyId, clinician, dateTime.ToString("yyyy-MM-dd"));
                }
                res.Message = valid == false ? "Please enter valid date like MM-dd-yyyy" : res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = valid == false ? StatusCodes.Status401Unauthorized : res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
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