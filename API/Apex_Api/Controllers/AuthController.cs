using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.RequestModel.RocketChatRequest;
using Apex.DataAccess.Response;
using Apex_Api.Service;
using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling.Internal;
using System;

namespace Apex_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class AuthController : ControllerBase
    {
        private IUserService _userService;
        private IConfiguration _Configuration;
        private readonly IHostingEnvironment _hostingEnv;
        private readonly IHostingEnvironment _environment;

        public AuthController(IUserService userService, IHostingEnvironment hostingEnv, IConfiguration Configuration, IHostingEnvironment environment)
        {
            _userService = userService;
            _hostingEnv = hostingEnv;
            _Configuration = Configuration;
            _environment = environment;
        }

        #region

        [AllowAnonymous]
        [HttpPost("Login")]
        public ApiResponse Login([FromBody] Login login)
        {
            var res = new ApiResponse();
            try
            {
                var password = Common.Encrypt(login.Password);
                var user = Common.Instances.User.Login(login.Email, password);

                if (user == null)
                {
                    res.Message = Constant.IncorrectEmailpass; res.Status = StatusCodes.Status404NotFound;
                    return res;
                }

                var subscriptionStatus = Common.ChecksubscriptionStatus(user.AgencyId);
                if (!subscriptionStatus && user.HasPaymentMethod && user.RoleName != Constant.SUPERADMIN)
                {
                    res.Message = Constant.SUBSCRIPTIONENDED; res.Status = StatusCodes.Status401Unauthorized;
                    return res;
                }

                //CHECK ROCKET CHAT ACCOUNT FOR USER
                if (user != null && user.RcUserId.IsNullOrWhiteSpace())
                {
                    var chatRes = RocketChatService.CreateUser(new ChatUserCreateRequest
                    {
                        email = user.Email,
                        name = user.FirstName,
                        password = user.Password,
                        username = Convert.ToString(user.Id)
                    });

                    if (chatRes.user != null)
                    {
                        user.RcUserId = chatRes.user._id;
                        user.RcPassword = user.Password;
                        user.RcUserName = chatRes.user.username;
                        Common.Instances.User.Save(user);
                    }
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

        [AllowAnonymous]
        [HttpGet("ForgotPassword")]
        public ApiResponse ForgotPassword(string email = "")
        {
            var res = new ApiResponse();
            try
            {
                var UserDetail = Common.Instances.User.CheckEmail(email);
                if (UserDetail == null) { res.Message = "Oops! Email does not exist."; res.Status = 402; return res; }
                var newPassword = Utility.FakerInstance.Random.AlphaNumeric(8);
                UserDetail.Password = Common.Encrypt(newPassword);
                if (UserDetail.Password != null) { Common.Instances.User.Save(UserDetail); }
                var getTemplate = EmailService.SendPasswordRecoveryEmail();
                var emailBody = getTemplate;
                var bodyHtml = emailBody.Replace("{Name}", $"{UserDetail.FirstName}")
                     .Replace("{Email}", UserDetail.Email)
                     .Replace("{Password}", newPassword);

                var wasSent = EmailService.SendEmail(UserDetail.Email, Constant.ForgotPasswordSubject, true, bodyHtml,
                    "", _Configuration);

                var message = (wasSent) ? "Email sent Successfully!" : "error in sent email!";
                res.Message = message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        #endregion
    }
}