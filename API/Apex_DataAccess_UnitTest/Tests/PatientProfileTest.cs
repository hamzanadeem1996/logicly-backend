using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using Xunit;

namespace Apex_DataAccess_UnitTest
{
    public class PatientProfileTest
    {
        private PatientProfileRepo repo;

        public PatientProfileTest()
        {
            repo = new PatientProfileRepo();
        }

        [Fact]
        public void ProfileEncryptionTest()
        {
            var res = repo.Get(56, 0);
            Assert.NotNull(res);
            Assert.True(res.Id > 0);

            var val = res.FirstName;
            res = repo.Save(res, 0);
            Assert.True(res.FirstName == val);
        }

        [Fact]
        public void Get()
        {
            var res = repo.Get(35, 0);
            Assert.NotNull(res);
            Assert.True(res.Id > 0);
        }

        [Fact]
        public void GetAll()
        {
            var idList = new List<int>();
            int pagenumber = 1;
            int pagesize = 25;
            var res = repo.GetAll(idList, idList, pagenumber, pagesize, "");
            Assert.NotNull(res.Items);
            Assert.NotEmpty(res.Items);
        }

        [Fact]
        public void UserSave()
        {
            var res = new PatientProfile { Admission = new DateTime(2021, 2, 1) };
            repo.Save(res, 0);
            Assert.True(res.Id > 0);
            // res.Recert = DateTime.UtcNow;
            // res.ThirtyDaysRelEval = DateTime.UtcNow;
        }
    }
}