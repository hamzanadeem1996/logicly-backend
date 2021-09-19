using Apex.DataAccess.Response;
using ElmahCore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Apex_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class CountryController : ControllerBase
    {
        [HttpGet("GetCountries")]
        public ApiResponse GetCountries(int pagenumber = 1, int pagesize = 250)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.CountryInst.GetCountries(pagenumber, pagesize);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet("GetStates")]
        public ApiResponse GetStates(int pagenumber = 1, int pagesize = 250, int countryid = 0)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.CountryInst.GetStates(pagenumber, pagesize, countryid);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet("GetCities")]
        public ApiResponse GetCities(int pagenumber = 1, int pagesize = 100, int stateid = 0)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.CountryInst.GetCities(pagenumber, pagesize, stateid);
                res.Message = Constant.Message;
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