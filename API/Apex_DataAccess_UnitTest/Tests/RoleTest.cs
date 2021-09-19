using Apex.DataAccess.Repositories;
using Xunit;

namespace Apex_DataAccess_UnitTest.Test
{
    public class RoleTest
    {
        private RoleRepo repo;

        public RoleTest()
        {
            repo = new RoleRepo();
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
    }
}