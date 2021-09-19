using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class AgencyRepo
    {
        public Agency GetAgency(int id)
        {
            var cmd = Sql.Builder.Select("a.*,p.Name as PlanName,p.StripePriceId").From("Agencies a");
            cmd.InnerJoin("Plans p").On("p.Id = a.PlanId");

            cmd.Where("a.Id=@0", id);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<Agency>(cmd) ?? new Agency();
                return result;
            }
        }

        public Page<Agency> GetAll(int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select("a.*,p.Name as PlanName").From("Agencies a");
            cmd.InnerJoin("Plans p").On("p.Id = a.PlanId");

            if (!string.IsNullOrWhiteSpace(query))
            {
                cmd.Where("a.Country like @0 OR a.State LIKE @0 OR a.City like @0 OR a.ZipCode like @0 OR " +
                    "a.Name LIKE @0 OR a.Email like @0 OR a.StripeCustomerId", $"%{query}%");
            }
            using (var db = Utility.Database)
            {
                var result = db.Page<Agency>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object SaveAgency(Agency res)
        {
            using (var db = Utility.Database)
            {
                if (res.Id == 0)
                {
                    res.AddedOn = DateTime.UtcNow;
                }
                res.LastModOn = DateTime.UtcNow;
                db.Save(res);
                return res;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                const string cmd = @"Delete FROM Agencies where Id =@0";
                var result = db.Execute(cmd, Id);
                return Convert.ToInt32(result);
            }
        }
    }
}