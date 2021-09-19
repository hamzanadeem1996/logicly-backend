using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using Xunit;

namespace Apex_DataAccess_UnitTest
{
    public class UserTest
    {
        private UserRepo repo;

        public UserTest()
        {
            repo = new UserRepo();
        }

        [Fact]
        public void GetUser()
        {
            var res = repo.Get(1);
            Assert.NotNull(res);
            Assert.True(res.Id > 0);
        }

        [Fact]
        public void GetAllUsersTest()
        {
            int pagenumber = 1;
            int pagesize = 25;
            var res = repo.GetAll(pagenumber, pagesize, "");
            Assert.NotNull(res.Items);
            Assert.NotEmpty(res.Items);
        }

        [Fact]
        public void UserSave()
        {
            User res = new User();
            res.FirstName = Faker.Name.First();
            res.LastName = Faker.Name.Last();
            res.Email = Faker.Internet.Email();
            res.Password = Constrants.Password;
            res.RoleId = 0;
            res.AddedBy = Constrants.AddedBy;
            res.LastModBy = Constrants.AddedBy;
            repo.Save(res);
            Assert.True(res.Id > 0);
        }
    }
}