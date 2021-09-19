using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class PlanPermissionRepo
    {
        public PlanPermission Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<PlanPermission>(id) ?? new PlanPermission();
                return result;
            }
        }

        public PlanPermission GetPlanPermissionByPlanName(string planName, string PermissionName)
        {
            var cmd = Sql.Builder.Select("pp.*,pl.Name as PlanName,p.Name as PermissionName").From("PlanPermissions pp");
            cmd.LeftJoin("Plans pl").On("pl.Id = pp.PlanId");
            cmd.LeftJoin("Permissions p").On("p.Id = pp.PermissionId");
            cmd.Where("pl.Name=@0", planName);
            cmd.Where("p.Name=@0", PermissionName);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PlanPermission>(cmd);
                return result;
            }
        }

        public Page<PlanPermission> GetAll(int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select("pp.*,pl.Name as PlanName,p.Name as PermissionName").From("PlanPermissions pp");
            cmd.LeftJoin("Plans pl").On("pl.Id = pp.PlanId");
            cmd.LeftJoin("Permissions p").On("p.Id = pp.PermissionId");
            cmd.Where("PlanId=@0", 1);
            cmd.OrderBy("pp.Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<PlanPermission>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object Save(PlanPermission planPermission, int currentLoggedUser)
        {
            using (var db = Utility.Database)
            {
                if (planPermission.Id == 0)
                {
                    planPermission.AddedOn = DateTime.UtcNow;
                    planPermission.AddedBy = currentLoggedUser;
                }
                planPermission.LastModOn = DateTime.UtcNow;
                planPermission.LastModBy = currentLoggedUser;
                db.Save(planPermission);
                return planPermission;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<PlanPermission>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}