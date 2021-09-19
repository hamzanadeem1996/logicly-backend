using Apex.DataAccess;
using Apex.DataAccess.Repositories;
using Apex_Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Apex.DataAccess.Utility;
using static Apex_Api.Service.GeoCodingService;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class UtilityController : ControllerBase
    {
        [HttpGet("GetRemaningDaysInMonth")]
        public int GetRemaningDaysInMonth(DateTime dateTime)
        {
            return DateTime.DaysInMonth(dateTime.Year, dateTime.Month) - dateTime.Day;
            //return DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month) - DateTime.UtcNow.Day;
        }

        [HttpGet("LocationInfo")]
        public object LocationInfo(double latitude, double longitude)
        {
            var mat = GoogleMapsService.GetLocationInfo(latitude, longitude);
            return mat;
        }

        [HttpGet("TestReferer")]
        public object TestReferer()
        {
            var me = Common.GetUserbyToken(HttpContext);
            var referer = Request.Headers["X-Referer"].ToString();
            return referer;
        }

        [HttpGet("GetWeekRangeBasedOnDate")]
        public string GetWeekRangeBasedOnDate(DateTime dateTime)
        {
            var me = Common.GetUserbyToken(HttpContext);
            var range = Utility.GetDateRangeOfWeek(dateTime);
            return range;
        }

        [HttpGet("ConvertTimeIntoMinute")]
        public object ConvertTimeIntoMinute(double startLat, double startLon, double endLat, double endLon)
        {
            return new GoogleMapsService().GetDistanceMatrix(startLat, startLon, endLat, endLon);
        }

        [HttpGet("GetNextWeekday")]
        public DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        [HttpGet("GetLeftDaysInWeek")]
        public int GetLeftDaysInWeek(DateTime start)
        {
            return Utility.GetLeftDaysInWeek(start)-1;
        }

        [HttpGet("GeoCoordinate")]
        public object GeoCoordinate(string address)
        {
            Task<Coordinates> task = new GeoCodingService().GeoCoordinate(address);

            return task;
        }


        


    }
}