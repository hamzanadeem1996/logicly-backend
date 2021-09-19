using Microsoft.AspNetCore.Mvc;

namespace Apex_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanPermissionController : ControllerBase
    {
        //[HttpGet("Get")]
        //public ApiResponse Get(int id)
        //{
        //    var res = new ApiResponse();
        //    try
        //    {
        //        res.Data = Common.PlanPermissionInst.Get(id);
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
        //        res.Data = Common.PlanPermissionInst.GetAll(pagenumber, pagesize, query);
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
    }
}