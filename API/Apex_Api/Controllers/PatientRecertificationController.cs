using Apex.DataAccess;
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
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class PatientRecertificationController : ControllerBase
    {
        [HttpGet("GetRecertificationById")]
        public ApiResponse GetRecertificationById([Required] int id = 0)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                res.Data = Common.Instances.RecertificationServiceInst.GetRecertificationbyId(id, me.Id);
                res.Message = res.Data != null ? Constant.Message : "Recertification not found.";
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpGet("GetNextRecert")]
        public ApiResponse GetNextRecert([Required] int patientId)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                var patient = Common.Instances.PatientProfileInst.Get(patientId, me.Id);
                var result = Common.Instances.RecertificationServiceInst.GetNextRecertification(patientId);
                result.RecertificationDate =
                    result.Id == 0 ? patient.Admission : result.RecertificationDate.AddDays(60);
                res.Data = result;
                res.Message = result.Id > 0 ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = result.Id > 0 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpGet("GetRecertification")]
        public ApiResponse GetRecertification(int patientId = 0, int clinicianId = 0)
        {
            var res = new ApiResponse();
            var me = GetUserbyToken(HttpContext);
            try
            {
                //if (me.RoleName != Constant.ADMIN) clinicianId = me.Id;
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;
                res.Data = Common.Instances.RecertificationServiceInst.GetRecertifications(patientId, me.Id);
                res.Message = res.Data != null ? Constant.Message : "Recertification not found.";
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [HttpPost("AddRecertification")]
        public ApiResponse AddRecertification(Recertification req)
        {
            var res = new ApiResponse();
            try
            {
                var me = GetUserbyToken(HttpContext);

                //Allow only SN OT PT SLP, Admin to add cert periods
                bool allow = Utility.CheckAddCertByRole(me.RoleName);
                if (!allow)
                    throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized,
                       $"You are not authorized to add a certification.");

                var patientCertifications = Common.Instances.RecertificationInst.GetRecertifications(req.PatientId);
                if (req.Id > 0)
                    patientCertifications.RemoveAll(x => x.Id == req.Id);

                var exist = new Recertification();
                foreach (var certification in patientCertifications)
                {
                    if (req.RecertificationDate <= certification.RecertificationDate.AddDays(58) &&
                        req.RecertificationDate >= certification.RecertificationDate)
                    {
                        exist = certification;
                        break;
                    }
                }

                //var exist = RecertificationServiceInst.GetNextRecertification(recertification.PatientId);
                if (exist.Id > 0)
                {
                    throw new HttpException(res.Status = (int)HttpStatusCode.BadRequest,
                        $"Certification overlap found. Please choose another date");
                }

                res.Data = Common.Instances.RecertificationServiceInst.SaveRecertification(req, me.Id);
                res.Message = res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = StatusCodes.Status500InternalServerError;
            }

            return res;
        }

        [HttpDelete("DeleteRecertification")]
        public ApiResponse DeleteRecertification([Required] int id)
        {
            var res = new ApiResponse();
            try
            {
                int status = 0;
                status = Common.Instances.RecertificationServiceInst.Delete(id);
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