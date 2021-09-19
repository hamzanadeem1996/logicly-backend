using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class ClinicianAvailabilityRepo
    {
        public Page<ClinicianAvailability> GetAll(int clinicianId, int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select("*").From("PatientAvailability");
            cmd.Where("ClinicianId=@0", clinicianId);
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<ClinicianAvailability>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object Save(ClinicianAvailability clinicianAvailability)
        {
            using (var db = Utility.Database)
            {
                db.Save(clinicianAvailability);
                return clinicianAvailability;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<ClinicianAvailability>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}