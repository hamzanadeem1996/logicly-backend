using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class PatientUnavailableTimeSlotRepo
    {
        public PatientUnavailableTimeSlot Get(int patientid)
        {
            var cmd = Sql.Builder.Select("*").From("PatientUnavailableTimeSlots");
            if (patientid > 0)
            {
                cmd.Where("PatientId=@0", patientid);
            }
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientUnavailableTimeSlot>(cmd);
                return result;
            }
        }

        public Page<PatientUnavailableTimeSlot> GetAll(List<int> Patientids, int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select("*").From("PatientUnavailableTimeSlots");

            if (Patientids.Count > 0)
            {
                cmd.Where("PatientId IN (@0)", Patientids);
            }
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<PatientUnavailableTimeSlot>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public PatientUnavailableTimeSlot Save(PatientUnavailableTimeSlot data)
        {
            using (var db = Utility.Database)
            {
                if (data.Id == 0)
                {
                    data.AddedOn = DateTime.UtcNow;
                }
                data.LastModOn = DateTime.UtcNow;
                db.Save(data);
                return data;
            }
        }
    }
}