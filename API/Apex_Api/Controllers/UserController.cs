using AgileObjects.AgileMapper.Extensions;
using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.RequestModel;
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
using Stripe;
using System;
using System.Linq;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private IConfiguration _Configuration;

        public UserController(IUserService userService, IConfiguration Configuration)
        {
            _userService = userService;
            _Configuration = Configuration;
        }

        #region

        [HttpPost("Me")]
        public ApiResponse Me()
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                var user = Common.Instances.User.Login(me.Email, me.Password);

                bool SubscriptionStatus = Common.ChecksubscriptionStatus(user.AgencyId);
                if (!SubscriptionStatus && user.HasPaymentMethod && user.RoleName != Constant.SUPERADMIN)
                {
                    res.Message = Constant.SUBSCRIPTIONENDED; res.Status = StatusCodes.Status401Unauthorized;
                    return res;
                }

                res.Data = user;
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

        [HttpGet("Get")]
        public ApiResponse Get(int id)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.User.Get(id);
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
        public ApiResponse GetAll(int pagenumber = 1, int pagesize = 20, string query = "", bool includeNone = false,
            bool includeAdmin = true, string roleName = "")
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);

                if (!includeAdmin)
                {
                    var getPermission = Common.Instances.PlanPermissionInst.GetPlanPermissionByPlanName(me.PlanName, $"Solo");
                    if (getPermission == null)
                        throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized, Constant.NOPERMISSION);
                }

                var result = Common.Instances.User.GetAll(pagenumber, pagesize, query, me.AgencyId, includeNone, includeAdmin);

                if (includeNone)
                { var user = new User { Id = 0, FirstName = "None", RoleName = roleName.ToUpper().Trim() }; result.Items.Insert(0, user); }

                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    result.Items = result.Items.Where(x => x.RoleName == roleName.ToUpper().Trim()).ToList();
                }

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

        [AllowAnonymous]
        [HttpPost("Save")]
        public ApiResponse Save(User user)
        {
            var res = new ApiResponse();
            try
            {
                var srv = new ServiceUser();
                var me = Common.GetUserbyToken(HttpContext);
                user.AgencyId = me.AgencyId;
                res.Data = srv.save(user, _userService, _Configuration);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                if (ex.Message == $"Duplicate entry '{user.Email}' for key 'UK_Users_Email'")
                {
                    res.Message = $"The Email '{user.Email}' is already taken.";
                    return res;
                }

                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }

            return res;
        }

        [AllowAnonymous]
        [HttpPost("QuickSignup")]
        public ApiResponse QuickSignup(QuickSignupRequest quickSignupRequest)
        {
            var res = new ApiResponse();
            try
            {
                var agencyService = new AgencyService();
                var agency = new Agency();
                agency = quickSignupRequest.Map().Over(agency);
                if (agency.StripeCustomerId.IsNullOrWhiteSpace())
                {
                    var cust = new StripeService().AddCustomerToStripe(ref agency);
                    agency.StripeCustomerId = cust.Id;
                }
                var saveAgency = agencyService.SaveAgency(agency, _userService, _Configuration, 0);
                res.Data = saveAgency;
                res.Message = res.Data != null ? Constant.QUICKSIGNUP : Utility.ResponseMessage.NotFound;
                res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
            }
            catch (Exception ex)
            {
                if (ex.Message == $"Duplicate entry '{quickSignupRequest.Email}' for key 'UK_Users_Email'")
                {
                    res.Message = $"The Email '{quickSignupRequest.Email}' is already taken.";
                    res.Status = StatusCodes.Status409Conflict; return res;
                }
                else if (ex.Message == $"Duplicate entry '{quickSignupRequest.Email}' for key 'UK_Agencies_Email'")
                {
                    res.Message = $"The Email '{quickSignupRequest.Email}' is already taken.";
                    res.Status = StatusCodes.Status409Conflict; return res;
                }

                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = StatusCodes.Status500InternalServerError;
            }
            return res;
        }

        [HttpDelete("CancelSubscription")]
        public ApiResponse CancelSubscription(int agencyId = 0)
        {
            var res = new ApiResponse();
            var me = Common.GetUserbyToken(HttpContext);
            try
            {
                var count = 0;
                me.AgencyId = me.RoleName == Constant.SUPERADMIN && agencyId > 0 ? agencyId : me.AgencyId;
                var agency = Common.Instances.AgencyInst.GetAgency(me.AgencyId);
                if (agency.Id > 0)
                {
                    var customer = new StripeService().GetCustomer(agency.StripeCustomerId);
                    var options = new SubscriptionListOptions
                    { Customer = customer.Id, };
                    var service = new SubscriptionService();
                    StripeList<Subscription> subscriptions = service.List(options);
                    foreach (var sub in subscriptions.Data)
                    {
                        if (sub != null)
                        {
                            service.Cancel(sub.Id);
                            res.Data = count++;
                        }
                    }
                }
                res.Message = res.Data != null ? "your subscription has been cancelled" : Utility.ResponseMessage.NotFound;
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

        [HttpDelete("Delete")]
        public ApiResponse Delete(int id)
        {
            var res = new ApiResponse();
            try
            {
                int status = 0;
                status = Common.Instances.User.Delete(id);
                res.Message = status >= 1 ? Constant.Message : Constant.NotFound;
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