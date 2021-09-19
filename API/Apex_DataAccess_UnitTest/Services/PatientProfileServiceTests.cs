using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using Apex_Api.Service;
using Xunit;

namespace Apex_DataAccess_UnitTest.Services
{
    public class PatientProfileServiceTests
    {
        [Fact]
        public void SaveTest()
        {
            var pp = new PatientProfileRepo().Get(125, 118);
            var srv = new PatientProfileService();
            var oldOt = pp.OT;
            pp.OT = 345;
            srv.Save(pp, new User {Id = 125});
            Assert.True(pp.Id > 0);
            
            pp.OT = oldOt;
            srv.Save(pp, new User {Id = 125});
            Assert.True(pp.Id > 0);
        }
    }
}