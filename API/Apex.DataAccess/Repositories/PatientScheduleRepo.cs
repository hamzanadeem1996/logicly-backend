using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apex.DataAccess.Repositories
{
    public class PatientScheduleRepo
    {
        public List<PatientSchedule> GetPatientSchedules(int PatientId,int ClinicianId,int RecertId)
        {
            var cmd = Sql.Builder.Select("*").From("PatientSchedule");
            cmd.Where("PatientId=@0 AND ClinicianId=@1 AND RecertId=@2 ", PatientId, ClinicianId, RecertId);
            using (var db = Utility.Database)
            {
                var result = db.Fetch<PatientSchedule>(cmd);
                return result;
            }
        }

    }
}
