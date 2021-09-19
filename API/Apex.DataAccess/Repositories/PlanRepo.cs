using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class PlanRepo
    {
        public Plan Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<Plan>(id) ?? new Plan();
                return result;
            }
        }

        public Page<Plan> GetAll(int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select("*").From("Plans");
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<Plan>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object Save(Plan plan, int currentLoggedUser)
        {
            using (var db = Utility.Database)
            {
                if (plan.Id == 0)
                {
                    plan.AddedOn = DateTime.UtcNow;
                    plan.AddedBy = currentLoggedUser;
                }
                plan.LastModOn = DateTime.UtcNow;
                plan.LastModBy = currentLoggedUser;
                db.Save(plan);
                return plan;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<Plan>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}