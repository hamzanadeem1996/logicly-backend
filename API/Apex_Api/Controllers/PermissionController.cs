using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class PermissionController : ControllerBase
    {
        #region

        //[HttpGet("Get")]
        //public ApiResponse Get(int id)
        //{
        //    var res = new ApiResponse();
        //    try
        //    {
        //        res.Data = Common.PermissionInst.Get(id);
        //        res.Message = res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
        //        res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpContext.RiseError(ex);
        //        res.Message = ex.Message;
        //    }
        //    return res;
        //}

        //[HttpGet("GetAll")]
        //public ApiResponse GetAll(int pagenumber = 1, int pagesize = 20, string query = "")
        //{
        //    var res = new ApiResponse();
        //    try
        //    {
        //        res.Data = Common.PermissionInst.GetAll(pagenumber, pagesize, query);
        //        res.Message = res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
        //        res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpContext.RiseError(ex);
        //        res.Message = ex.Message;
        //    }
        //    return res;
        //}

        //[HttpPost("Save")]
        //public ApiResponse Save(Permission permission)
        //{
        //    var res = new ApiResponse();
        //    try
        //    {
        //        var me = Common.GetUserbyToken(HttpContext);
        //        if (me.RoleName != Constant.SUPERADMIN && me.RoleName != Constant.ADMIN)
        //            throw new HttpException((int)HttpStatusCode.Unauthorized, Utility.ResponseMessage.Unauthorized);

        //        var validator = new InlineValidator<Permission>();
        //        validator.RuleSet("PermissionValidator", () => {
        //            validator.RuleFor(x => x.Name).NotNull().NotEmpty();
        //        });
        //        var valRes = validator.Validate(permission, ruleSet: "PermissionValidator");
        //        if (!valRes.IsValid)
        //        {
        //            return res.PrepareInvalidRequest(ref valRes);
        //        }
        //        res.Data = Common.PermissionInst.Save(permission, me.Id);
        //        res.Message = res.Data != null ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
        //        res.Status = res.Data != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpContext.RiseError(ex);
        //        res.Message = ex.Message;
        //    }
        //    return res;
        //}

        //[HttpDelete("Delete")]
        //public ApiResponse Delete(int id)
        //{
        //    var res = new ApiResponse();
        //    try
        //    {
        //        int status = 0;
        //        status = Common.PermissionInst.Delete(id);
        //        res.Message = status > 0 ? Utility.ResponseMessage.Ok : Utility.ResponseMessage.NotFound;
        //        res.Status = status > 0 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpContext.RiseError(ex);
        //        res.Message = ex.Message;
        //    }
        //    return res;
        //}
        #endregion
    }
}