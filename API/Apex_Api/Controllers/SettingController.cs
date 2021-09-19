using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using static Apex_Api.Common;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class SettingController : ControllerBase
    {
        #region

        [HttpGet("Get")]
        public ApiResponse GetSetting(int clinicianId = 0)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);

                me.Id = me.RoleName == Constant.ADMIN && clinicianId > 0 ? clinicianId : me.Id;

                res.Data = Common.Instances.SettingsRepoInst.Get(me.Id);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost("Save")]
        public ApiResponse SaveSetting(Setting setting)
        {
            var res = new ApiResponse();
            try
            {
                var me = Common.GetUserbyToken(HttpContext);

                var getSetting = Common.Instances.SettingsRepoInst.Get(me.Id);

                var existVisitInWeekend = Common.Instances.VisitScheduleRepoInst.CheckVisitExistInWeekend(me.Id);

                if (getSetting.Id > 0 && existVisitInWeekend != null &&
                    getSetting.IncludeWeekendsInWeekView != setting.IncludeWeekendsInWeekView)
                {
                    throw new HttpException(res.Status = 402,
                        "You have patients scheduled on weekends. Please remove scheduled patients before changing settings.");
                }

                var getAllVisitByUserId = Common.Instances.VisitScheduleRepoInst.GetAll(1, 1000, "", me.Id, 0).Items;
                if (getAllVisitByUserId.Count > 0)
                {
                    foreach (var item in getAllVisitByUserId)
                    {
                        int GetDefaultsessiontime = item.colorType == "E" ? setting.EvaluationSessionLength : //Evaluation
                        item.colorType == "R" ? setting.RecertSessionLength : //Recert
                        item.colorType == "D" ? setting.DischargeSessionLength : //Discharge
                        item.colorType == "30" ? setting.ThirtyDayReEvalSessionLength : // 30DRE
                        item.colorType == "RV" ? setting.TreatmentSessionLength : // Routine Visit
                        30;  //Default 30
                        //item.Start = item.Start;
                        item.End = item.Start.AddMinutes(GetDefaultsessiontime);
                        Common.Instances.VisitScheduleRepoInst.Save(item);
                    }
                }
                res.Data = Common.Instances.SettingsRepoInst.Save(setting, me.Id);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Duplicate entry '" + setting.UserId + "' for key 'Id'")
                {
                    res.Message = "The UserId '" + setting.UserId + "' is already taken.";
                    return res;
                }
                else
                {
                    HttpContext.RiseError(ex);
                    res.Message = ex.Message;
                }
            }
            return res;
        }

        #endregion
    }
}