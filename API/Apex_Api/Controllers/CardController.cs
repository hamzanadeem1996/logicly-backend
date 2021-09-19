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
using System;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class CardController : ControllerBase
    {
        [HttpPost("GetCard")]
        public ApiResponse GetCard()
        {
            var res = new ApiResponse();
            try
            {
                var srv = new CardService();
                var me = Common.GetUserbyToken(HttpContext);

                if (me.RoleName != Constant.ADMIN)
                    throw new UnauthorizedAccessException();

                var data = Common.Instances.CardInst.Get(me.AgencyId);

                if (data == null)
                    throw new HttpException((int)HttpStatusCode.NotFound,
                        $"Card {Utility.ResponseMessage.NotFound}");

                data.CardNumber = Encryption.Decrypt(data.CardNumber);

                data.CardNumber = $"************{ data.CardNumber.Substring(data.CardNumber.Length - 4, 4)}";

                res.Data = data;
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

        [HttpPost("SaveCard")]
        public ApiResponse SaveCard(Card card)
        {
            var res = new ApiResponse();
            try
            {
                var srv = new CardService();
                var me = Common.GetUserbyToken(HttpContext);

                if (me.RoleName != Constant.ADMIN)
                    throw new UnauthorizedAccessException();

                res.Data = srv.Save(card, me.AgencyId, me.Id);

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
    }
}