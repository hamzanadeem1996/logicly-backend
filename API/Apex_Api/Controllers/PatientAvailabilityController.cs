using Apex.DataAccess;
using Apex.DataAccess.RequestModel;
using Apex.DataAccess.Response;
using Apex_Api.Service;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class PatientAvailabilityController : ControllerBase
    {
        #region

        [HttpGet("Get")]
        public ApiResponse Get([Required] int id)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.PatientAvailabilityInst.Get(id);
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

        [HttpGet("GetPatientAvailability")]
        public ApiResponse GetPatientAvailability([Required] int patientId, int pageNumber = 1, int pageSize = 20, string query = "")
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.PatientAvailabilityInst.GetAll(patientId, pageNumber, pageSize, query);
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

        [HttpPost("Save")]
        public ApiResponse Save(PatientAvailabilityRequest patientAvailabilityRequest)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = new PatientUnavailableServie().Save(patientAvailabilityRequest);
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

        [HttpDelete("Delete")]
        public ApiResponse Delete(int id)
        {
            var res = new ApiResponse();
            try
            {
                int status = 0;
                status = Common.Instances.PatientAvailabilityInst.Delete(id);
                res.Message = status == 1 ? Constant.Message : Constant.NotFound;
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