using Apex.DataAccess.Models;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class MyDocumentsController : ControllerBase
    {
        public class Apires
        {
            public List<Result> Data { get; set; }
            public string Message { get; set; }
        }

        public class Result
        {
            public string VisitType { get; set; }
            public List<PatientDate> Events { get; set; }
        }

        [HttpGet("MyDocumentsDue")]
        public Apires MyDocumentsDue(DateTime startdate, int patientid = 0, int clinicianId = 0)  // if patient id greater than 0 date no req. return all patient schedule
        {
            var me = Common.GetUserbyToken(HttpContext);
            var res = new Apires();

            try
            {
                //if (me.RoleName != Constant.ADMIN) clinicianId = me.Id;
                var ClinicianId = new List<int>();
                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                ClinicianId.Add(me.Id);

                var visitTypes = new VisitType().GetAll();
                var getPatientsbydate = Common.Instances.PatientProfile.PatientDates(ClinicianId, startdate.Date.ToString("yyyy-MM-dd"),
                    startdate.Date.AddDays(6).ToString("yyyy-MM-dd"), patientid, false, "Discharged", me.AgencyId);

                res.Data = new List<Result>();

                foreach (var item in visitTypes)
                {
                    var result = new Result();
                    result.VisitType = item.Name;
                    result.Events = new List<PatientDate>();
                    var evtlist = getPatientsbydate.Where(x => x.Type == item.Name).ToList();
                    foreach (var item2 in evtlist) result.Events.Add(item2);
                    res.Data.Add(result);
                }

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