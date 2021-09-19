using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class UserRepo
    {
        public User Get(int id)
        {
            var cmd = Sql.Builder.Select($@"u.*,r.Name as RoleName,s.IncludeWeekendsInWeekView").From("Users u");
            cmd.LeftJoin("Settings s").On("s.UserId=u.Id");
            cmd.InnerJoin("Roles r").On("r.Id=u.RoleId");
            cmd.Where("u.Id=@0", id);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<User>(cmd);
                return result;
            }
        }

        public Page<User> GetAll(int pagenumber, int pagesize, string query = "", int agencyid = 0,
            bool includeNone = false, bool includeAdmin = true)
        {
            var cmd = Sql.Builder.Select($@"u.*,r.Name as RoleName").From("Users u");
            cmd.InnerJoin("Roles r").On("r.Id=u.RoleId");
            cmd.Where("r.Name!=@0 AND u.AgencyId=@1", "SUPERADMIN", agencyid);

            if (!includeAdmin)
                cmd.Where("r.Name!=@0", "ADMIN");

            if (!string.IsNullOrWhiteSpace(query))
            {
                cmd.Where("u.Id like @0 OR u.FirstName LIKE @0 OR u.LastName like @0 OR u.RoleId like @0",
                    $"%{query}%");
            }

            cmd.OrderBy("u.Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<User>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object Save(User userinfo)
        {
            using (var db = Utility.Database)
            {
                if (userinfo.Id == 0) userinfo.AddedOn = DateTime.UtcNow;

                userinfo.LastModOn = DateTime.UtcNow;

                db.Save(userinfo);
                return userinfo;
            }
        }

        public IEnumerable<User> GetUsersMissingRocketChatAccount()
        {
            var cmd = Sql.Builder.Select(@"*
                    from Users u
                    where u.RcUserName NOT LIKE '00%'
                    OR u.RcUserName IS NULL OR u.RcUserName =''");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<User>(cmd);
                return result;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                const string cmd = @"Delete from Users where Id=@0;
                                     Delete FROM PatientVisitSchedule where NurseId=@0;
                                     Delete FROM Settings where UserId =@0";
                var result = db.Execute(cmd, Id);
                return Convert.ToInt32(result);
            }
        }

        public User Login(string email = "", string password = "")
        {
            var cmd = Sql.Builder
                .Select("u.*,r.Name as RoleName,a.MaxSessionHours,p.Name as PlanName, a.Name as AgencyName, a.Latitude as AgencyLatitude, a.Longitude as AgencyLongitude")
                .From("Users u");
            cmd.LeftJoin("Roles r").On("r.Id=u.RoleId");
            cmd.LeftJoin("Agencies a").On("a.Id=u.AgencyId");
            cmd.LeftJoin("Plans p").On("p.Id=a.PlanId");
            cmd.Where("u.Email=@0", email);
            cmd.Where("u.Password=@0", password);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<User>(cmd);
                return result;
            }
        }

        public User CheckEmail(string email = "")
        {
            var cmd = Sql.Builder.Select($@"u.*,r.Name as RoleName,p.Name as PlanName,s.IncludeWeekendsInWeekView")
                .From("Users u");
            cmd.LeftJoin("Roles r").On("r.Id=u.RoleId");
            cmd.LeftJoin("Agencies a").On("a.Id=u.AgencyId");
            cmd.LeftJoin("Plans p").On("p.Id=a.PlanId");
            cmd.LeftJoin("Settings s").On("s.UserId=u.Id");
            cmd.Where("u.Email=@0", email);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<User>(cmd);
                return result;
            }
        }

        public User Getpasswordbyuserid(int id)
        {
            var cmd = Sql.Builder.Select("Password,Token").From("Users");
            cmd.Where("Id=@0", id);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<User>(cmd);
                return result;
            }
        }

        public List<User> GetAllUserByMultipleIds(List<int> ids, string query = "")
        {
            var cmd = Sql.Builder
                .Select(
                    $@"r.Name as RoleName,u.Id,u.RoleId,u.FirstName,u.LastName,u.Email,u.AddedBy,u.LastModBy,u.AddedOn,
                       u.LastModOn,u.CityId,u.Address,u.Lat,u.Long,u.PhoneNumber,u.CountryCode")
                .From("Users u");
            cmd.InnerJoin("Roles r").On("r.Id=u.RoleId");
            //cmd.Where("r.Name!=@0", "ADMIN");
            if (ids.Count > 0)
            {
                cmd.Where("u.Id IN (@0)", ids);
            }
            if (!string.IsNullOrWhiteSpace(query))
            {
                cmd.Where("u.Id like @0 OR u.FirstName LIKE @0 OR u.LastName like @0 OR u.RoleId like @0",
                    $"%{query}%");
            }
            using (var db = Utility.Database)
            {
                var result = db.Fetch<User>(cmd);
                return result;
            }
        }
    }
}