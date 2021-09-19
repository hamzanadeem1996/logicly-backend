using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using System;
using Xunit;

namespace Apex_DataAccess_UnitTest.Test
{
    public class SettingTest
    {
        private SettingRepo repo;

        public SettingTest()
        {
            repo = new SettingRepo();
        }

        [Fact]
        public void GetSetting()
        {
            var res = repo.Get(1);
            Assert.NotNull(res);
            Assert.True(res.Id > 0);
        }

        [Fact]
        public void SettingSave()
        {
            // Generate Random Userid
            Random random = new Random();
            Setting res = new Setting();
            res.UserId = random.Next(9999);
            res.Units = Constrants.Units;
            res.RoutingApp = Constrants.RoutingApp;
            res.TreatmentSessionLength = 30;
            res.EvaluationSessionLength = 30;
            res.AdmissionSessionLength = 30;
            res.IncludeWeekendsInWeekView = Constrants.IncludeWeekendsInWeekView;
            res.DistanceCalculator = Constrants.DistanceCalculator;
            res.WorkingHours = "09:00-06:30";
            res.PatinetNameFormat = Constrants.PatinetNameFormat;
            res.ColorCoding = Constrants.ColorCoding;
            repo.Save(res);
            Assert.True(res.Id > 0);
        }
    }
}