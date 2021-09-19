using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class RecertificationRepo
    {
        public List<Recertification> GetRecertifications(int patientid)
        {
            var cmd = Sql.Builder.Select("r.*,GROUP_CONCAT(IFNULL(ps.GeneratedVisitCode, 'N/A')) as Frequency," +
                "Concat(u.FirstName,' ',u.LastName) as NurseName").From("Recertifications r");
            cmd.LeftJoin("PatientSchedule ps").On("ps.RecertId = r.Id");
            cmd.LeftJoin("Users u").On("u.Id = r.AddedBy");
            cmd.Where("r.PatientId=@0", patientid);
            cmd.GroupBy("r.Id");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<Recertification>(cmd);
                return result;
            }
        }

        public Recertification Save(Recertification recertification, int nurseid)
        {
            using (var db = Utility.Database)
            {
                if (recertification.Id == 0)
                {
                    recertification.AddedOn = DateTime.UtcNow;
                }
                recertification.AddedBy = nurseid;
                recertification.LastModOn = DateTime.UtcNow;
                recertification.LastModBy = nurseid;
                db.Save(recertification);
                return recertification;
            }
        }
    }
}