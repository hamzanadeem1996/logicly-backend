using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class PatientDateRepo
    {
        public PatientDate Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<PatientDate>(id) ?? new PatientDate();
                return result;
            }
        }

        public PatientDate GetPatientDatesByType(int patientid, int clinicianId, int recertId, string type = "")
        {
            var cmd = Sql.Builder.Select($@"*").From("PatientDates");
            cmd.Where("PatientId=@0 AND RecertId=@1", patientid, recertId);
            cmd.Where("Type=@0 AND ClinicianId =@1", type, clinicianId);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientDate>(cmd) ?? new PatientDate();
                return result;
            }
        }

        public List<PatientDate> GetMultipleVisits(int patientid, int clinicianId, int recertId, string type = "")
        {
            var cmd = Sql.Builder.Select($@"pd.*,CONCAT_WS(' ', u.FirstName, u.LastName ) as NurseName,
               CONCAT(
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(pp.LastName),'{Encryption.Key}') AS CHAR (150))
                 ) AS PatientFullName").From("PatientDates pd");

            cmd.LeftJoin("Users u").On("u.Id = pd.ClinicianId");
            cmd.LeftJoin("PatientProfiles pp").On("pp.Id = pd.PatientId");
            cmd.Where("pd.PatientId=@0 AND pd.Type=@1 AND pd.RecertId=@2", patientid, type, recertId);
            if (clinicianId > 0) { cmd.Where("pd.ClinicianId=@0", clinicianId); }
            cmd.GroupBy("pd.Id");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<PatientDate>(cmd);
                return result;
            }
        }

        public int DeleteDischarge(int patientId, int recertId, int clinicianId)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from PatientDates");
                cmd.Where("PatientId=@0 AND RecertId=@1 AND ClinicianId=@2 ", patientId, recertId, clinicianId);
                cmd.Where("Type='Discharge'");
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }



        public PatientDate GetDischargeIsScheduled(int patientid, int clinicianId,int RecertId)
        {
            var cmd = Sql.Builder.Select($@"*").From("PatientDates");
            cmd.Where("PatientId=@0 AND ClinicianId=@1 AND RecertId=@2", patientid, clinicianId, RecertId);
            cmd.Where("Type ='Discharge' AND IsAddedToSchedule = true");
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientDate>(cmd) ?? new PatientDate();
                return result;
            }
        }


        public object Save(PatientDate patientDate)
        {
            using (var db = Utility.Database)
            {
                db.Save(patientDate);
                return patientDate;
            }
        }


        // GET LAST PATIENT SCHEDULE ADDED DATE BASED ON RECERT
        public List<PatientDate> GetLastPatientDate(int patientid, int clinicianId, int recertId)
        {
            var cmd = Sql.Builder.Select($@"*").From("PatientDates");
            cmd.Where($"PatientId={patientid} AND ClinicianId={clinicianId} AND RecertId = {recertId}");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<PatientDate>(cmd);
                return result;
            }
        }

        // FREQUENCY TYPE UPDATE

    }
}