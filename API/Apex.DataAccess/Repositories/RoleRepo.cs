using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class RoleRepo
    {
        public Role Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<Role>(id) ?? new Role();
                return result;
            }
        }

        public Page<Role> GetAll(int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select("*").From("Roles");
            cmd.Where("Name!=@0", "SUPERADMIN");
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<Role>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<Role>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}