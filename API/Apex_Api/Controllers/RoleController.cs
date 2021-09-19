using Apex.DataAccess.Response;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class RoleController : ControllerBase
    {
        #region

        [HttpGet("Get")]
        public ApiResponse Get(int id)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.roles.Get(id);
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
        public ApiResponse GetAll(int pagenumber = 1, int pagesize = 20, string query = "")
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.roles.GetAll(pagenumber, pagesize, query);
                res.Message = Constant.Message;
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
                status = Common.Instances.roles.Delete(id);
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