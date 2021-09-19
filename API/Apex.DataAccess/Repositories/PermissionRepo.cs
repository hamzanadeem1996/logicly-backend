using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class PermissionRepo
    {
        public Permission Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<Permission>(id) ?? new Permission();
                return result;
            }
        }

        public Page<Permission> GetAll(int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select("*").From("Permissions");
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<Permission>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object Save(Permission permission, int currentLoggedUser)
        {
            using (var db = Utility.Database)
            {
                if (permission.Id == 0)
                {
                    permission.AddedOn = DateTime.UtcNow;
                    permission.AddedBy = currentLoggedUser;
                }
                permission.LastModOn = DateTime.UtcNow;
                permission.LastModBy = currentLoggedUser;
                db.Save(permission);
                return permission;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<Permission>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}