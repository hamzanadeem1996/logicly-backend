using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using Apex_Api.Service;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling.Internal;
using System;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class AgencyController : ControllerBase
    {
        private IUserService _userService;
        private IConfiguration _Configuration;

        public AgencyController(IUserService userService, IConfiguration Configuration)
        {
            _userService = userService;
            _Configuration = Configuration;
        }

        [HttpGet("Get")]
        public ApiResponse Get(int id)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                if (me.Id != id && me.RoleName != Constant.SUPERADMIN)
                    throw new HttpException((int)HttpStatusCode.Unauthorized, Utility.ResponseMessage.Unauthorized);

                res.Data = Common.Instances.AgencyInst.GetAgency(id);
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

        [HttpGet("GetAll")]
        public ApiResponse GetAll(int pagenumber = 1, int pagesize = 20, string query = "")
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                if (me.RoleName != Constant.SUPERADMIN)
                    throw new HttpException((int)HttpStatusCode.Unauthorized, Utility.ResponseMessage.Unauthorized);

                res.Data = Common.Instances.AgencyInst.GetAll(pagenumber, pagesize, query);
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

        //We have GetAgencySettings API for user? It would return settings for
        //the current agency user is associated with, based on token
        [HttpGet("GetAgencySetting")]
        public ApiResponse GetAgencySetting()
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                res.Data = Common.Instances.AgencyInst.GetAgency(me.AgencyId);
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
        public ApiResponse Save(Agency result)
        {
            var res = new ApiResponse();
            try
            {
                var srv = new AgencyService();
                var usr = new ServiceUser();

                object data = "";

                var me = Common.GetUserbyToken(HttpContext);

                if (me.RoleName == Constant.USER)
                    throw new HttpException((int)HttpStatusCode.Unauthorized, Utility.ResponseMessage.Unauthorized);
                else if (me.RoleName == Constant.ADMIN)
                {
                    var getagency = Common.Instances.AgencyInst.GetAgency(me.AgencyId);
                    result.Id = getagency.Id;
                    result.StripeCustomerId = getagency.StripeCustomerId;
                }

                if (result.StripeCustomerId.IsNullOrWhiteSpace())
                {
                    var cust = new StripeService().AddCustomerToStripe(ref result);
                    result.StripeCustomerId = cust.Id;
                }

                data = srv.SaveAgency(result, _userService, _Configuration, me.Id);

                res.Data = data;
                res.Message = res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
            }
            catch (Exception ex)
            {
                if (ex.Message == $"Duplicate entry '{result.Email}' for key 'UK_Agencies_Email'")
                {
                    res.Message = $"The Email '{result.Email}' is already taken.";
                    res.Status = StatusCodes.Status409Conflict; return res;
                }
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = StatusCodes.Status500InternalServerError;
            }
            return res;
        }

        [HttpDelete("Delete")]
        public ApiResponse Delete(int id)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                if (me.Id != id && me.RoleName != Constant.SUPERADMIN)
                    throw new HttpException((int)HttpStatusCode.Unauthorized, Utility.ResponseMessage.Unauthorized);

                int status = 0;
                status = Common.Instances.AgencyInst.Delete(id);
                res.Message = status >= 1 ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
                res.Status = status >= 1 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
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