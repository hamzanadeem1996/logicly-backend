using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using Apex_Api.Service;
using ElmahCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class ClinicianAvailabilityController : ControllerBase
    {
        #region

        [HttpGet("GetClinicianAvailability")]
        public ApiResponse GetClinicianAvailability([Required] int clinicianId, int pageNumber = 1, int pageSize = 20, string query = "")
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.ClinicianAvailabilityInst.GetAll(clinicianId, pageNumber, pageSize, query);
                res.Message = Utility.ResponseMessage.Ok;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost("Save")]
        public ApiResponse Save(ClinicianAvailability clinicianAvailability)
        {
            var res = new ApiResponse();
            try
            {
                var validator = new InlineValidator<ClinicianAvailability>();

                validator.RuleSet("ClinicianAvailabilityValidator", () =>
                {
                    validator.RuleFor(x => x.ClinicianId).NotNull().NotEmpty();
                    validator.RuleFor(x => x.WeekDayNo).NotNull().NotEmpty();
                    validator.RuleFor(x => x.StartHour).NotNull().NotEmpty();
                    validator.RuleFor(x => x.EndHour).NotNull().NotEmpty();
                });
                var valRes = validator.Validate(clinicianAvailability, ruleSet: "ClinicianAvailabilityValidator");
                if (!valRes.IsValid)
                {
                    return res.PrepareInvalidRequest(ref valRes);
                }
                res.Data = Common.Instances.ClinicianAvailabilityInst.Save(clinicianAvailability);
                res.Message = Utility.ResponseMessage.Ok;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpDelete("Delete")]
        public ApiResponse Delete(int id)
        {
            var res = new ApiResponse();
            try
            {
                int status = 0;
                status = Common.Instances.ClinicianAvailabilityInst.Delete(id);
                res.Message = status == 1 ? Constant.Message : Constant.NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet("GetClinicianLocations")]
        public ApiResponse GetClinicianLocations([Required] double latitude, [Required] double longitude,
            int pageNumber = 1, int pageSize = 20)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                var referer = Request.Headers["X-Referer"].ToString();

                if (referer == Constant.MODEWEB && referer != null)
                {
                    var getPermission = Common.Instances.PlanPermissionInst.GetPlanPermissionByPlanName(me.PlanName, $"Map.Patients"); ;
                    if (getPermission == null)
                    {
                        throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized, Constant.NOPERMISSION);
                    }
                }

                var srv = new ClinicianLocationService();
                res.Data = srv.GetClinicianLocations(latitude, longitude, pageNumber, pageSize, me.AgencyId);
                res.Message = Utility.ResponseMessage.Ok;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost("Ping")]
        public ApiResponse Ping(ClinicianLocation clinicianLocation)
        {
            var res = new ApiResponse();
            try
            {
                var validator = new InlineValidator<ClinicianLocation>();
                validator.RuleSet("ClinicianAvailabilityValidator", () =>
                {
                    validator.RuleFor(x => x.Latitude).NotNull().NotEmpty();
                    validator.RuleFor(x => x.Longitude).NotNull().NotEmpty();
                });
                var valRes = validator.Validate(clinicianLocation, ruleSet: "ClinicianLocationValidator");
                if (!valRes.IsValid)
                {
                    return res.PrepareInvalidRequest(ref valRes);
                }
                var srv = new ClinicianLocationService();
                res.Data = srv.SaveLocation(clinicianLocation);
                res.Message = Utility.ResponseMessage.Ok;
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