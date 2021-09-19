using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using System;
using Xunit;

namespace Apex_DataAccess_UnitTest.Test
{
    public class PatientVisitHistoryTest
    {
        private PatientVisitHistoryRepo repo;

        public PatientVisitHistoryTest()
        {
            repo = new PatientVisitHistoryRepo();
        }

        [Fact]
        public void Get()
        {
            var res = repo.Get(1);
            Assert.NotNull(res);
            Assert.True(res.Id > 0);
        }

        [Fact]
        public void GetAll()
        {
            int pagenumber = 1;
            int pagesize = 25;
            var res = repo.GetAll(pagenumber, pagesize, "");
            Assert.NotNull(res.Items);
            Assert.NotEmpty(res.Items);
        }

        [Fact]
        public void PatientVisitHistorySave()
        {
            PatientVisitHistory res = new PatientVisitHistory();
            res.PatientId = 26;
            res.VisitDate = DateTime.UtcNow;
            res.Status = "Active";
            res.Notes = Faker.Lorem.Sentence(1);
            res.AddedBy = Constrants.AddedBy;
            res.LastModBy = Constrants.AddedBy;
            repo.Save(res);
            Assert.True(res.Id > 0);
        }
    }
}