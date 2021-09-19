using Apex.DataAccess.RequestModel.RocketChatRequest;
using Apex.DataAccess.Response;
using Apex.DataAccess.ResponseModel.RocketChatResponse;
using Apex_Api.Service;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class RocketChatController : ControllerBase
    {
        [HttpGet("Info")]
        public ApiResponse Info()
        {
            var res = new ApiResponse();
            try
            {
                res.Data = RocketChatService.Info();
                res.Message = Constant.Message;
                res.Status = res.Data != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = (int)HttpStatusCode.InternalServerError;
            }

            return res;
        }

        [HttpPost("Login")]
        public ApiResponse Login(ChatLoginRequest req)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = RocketChatService.Login(req.user, req.password);
                res.Message = Constant.Message;
                res.Status = res.Data != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = (int)HttpStatusCode.InternalServerError;
            }

            return res;
        }

        [HttpPost("CreateMissingRocketChatAccounts")]
        public ApiResponse CreateMissingRocketChatAccounts()
        {
            var res = new ApiResponse();
            try
            {
                var count = 0;
                var users = Common.Instances.User.GetUsersMissingRocketChatAccount();
                foreach (var user in users)
                {
                    var chatRes = RocketChatService.CreateUser(new ChatUserCreateRequest
                    {
                        email = user.Email,
                        name = user.FirstName,
                        password = user.Password,
                        username = Convert.ToString(user.Id)
                    });

                    if (chatRes.user == null) continue;
                    user.RcUserId = chatRes.user._id;
                    user.RcPassword = user.Password;
                    user.RcUserName = chatRes.user.username;
                    Common.Instances.User.Save(user);
                    count++;
                }

                res.Data = new object();
                res.Message = $"{count} missing accounts created in RocketChat";
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = (int)HttpStatusCode.InternalServerError;
            }
            return res;
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        [HttpGet("GetRooms")]
        public ApiResponse GetRooms()
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);
                var referer = Request.Headers["X-Referer"].ToString();

                var getPermission = Common.Instances.PlanPermissionInst.GetPlanPermissionByPlanName(me.PlanName, $"CHAT.{referer.ToUpper()}"); ;
                if (getPermission == null)
                {
                    throw new HttpException(res.Status = (int)HttpStatusCode.Unauthorized, Constant.NOPERMISSION);
                }
                var rooms = new List<RoomsResponse>();
                if (me.RoleName.ToLower() == "admin" || me.RoleName.ToLower() == "user")
                {
                    var data = RocketChatService.SubscriptionsGet(ref me);
                    data.update = data.update.Where(x => x.fname != null).ToList(); //ONLY GET PERSONAL CONVERSATONS
                    foreach (var chatUser in data.update)
                    {
                        if (IsDigitsOnly(chatUser.name))
                        {
                            var room = new RoomsResponse();
                            var user = Common.Instances.User.Get(Convert.ToInt32(chatUser.name));
                            if (user == null) continue;
                            room.Role = user.RoleName.ToUpper();
                            room.Id = chatUser._id;
                            room.Name = user.FullName;
                            room.Role = user.RoleName;
                            room.IsBold = true;
                            room.UnreadCount = chatUser.unread;
                            room.RoomId = chatUser.rid;
                            room.UpdatedAt = chatUser._updatedAt;
                            room.Alert = chatUser.alert;
                            room.ChatUsername = user.RcUserName;
                            rooms.Add(room);
                        }
                    }

                    var allUsers = RocketChatService.UserList(ref me);
                    allUsers.users = allUsers.users.Where(x => x.username != null)
                        .ToList();
                    foreach (var chatUser in allUsers.users)
                    {
                        if (IsDigitsOnly(chatUser.username))
                        {
                            var room = new RoomsResponse();
                            var user = Common.Instances.User.Get(Convert.ToInt32(chatUser.username));
                            if (user == null) continue;
                            room.Role = user.RoleName.ToUpper();
                            room.Id = chatUser._id;
                            room.ChatUsername = user.RcUserName;
                            room.Name = user.FullName;
                            room.IsBold = false;
                            rooms.Add(room);
                        }
                    }
                }

                res.Data = rooms.GroupBy(x => x.ChatUsername).Select(y => y.First()).OrderBy(z => z.Name);
                res.Status = res.Data != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
                res.Status = res.Status != 0 ? res.Status : (int)HttpStatusCode.InternalServerError;
            }

            return res;
        }
    }
}