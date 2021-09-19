using Apex.DataAccess.Repositories;
using Xunit;

namespace Apex_DataAccess_UnitTest.Test
{
    public class FileTest
    {
        private FileRepo repo;

        public FileTest()
        {
            repo = new FileRepo();
        }

        [Fact]
        public void Get()
        {
            var res = repo.Get(1);
            Assert.NotNull(res);
            Assert.True(res.Id > 0);
        }
    }
}