using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.ResponseModel;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex_Api.Service
{
    public class RecertificationService
    {
        public List<RecertificationResponse> GetRecertifications(int patientid, int currentLoggedUser = 0)
        {
            var cmd = Sql.Builder.Select($@"r.*,
               IFNULL(
                (Select  GROUP_CONCAT(IFNULL(ps.GeneratedVisitCode, 'N/A'))
                from Recertifications rr
                INNER JOIN
                PatientSchedule ps ON ps.RecertId = rr.Id
                WHERE
                rr.PatientId = r.PatientId AND ps.ClinicianId = {currentLoggedUser}),'N/A') AS Frequency,

                CONCAT_WS(' ', u.FirstName, u.LastName ) as NurseName")
                .From("Recertifications r");

            //cmd.LeftJoin("PatientSchedule ps").On("ps.RecertId = r.Id");
            cmd.LeftJoin("Users u").On("u.Id = r.AddedBy");
            cmd.Where("r.PatientId=@0", patientid);
            cmd.GroupBy("r.Id");

            using (var db = Utility.Database)
            {
                var result = db.Fetch<RecertificationResponse>(cmd);
                return result;
            }
        }

        public RecertificationResponse GetRecertificationbyId(int id, int currentLoggedUser = 0, int patientid = 0)
        {
            var cmd = Sql.Builder.Select($@"r.*,
               IFNULL(
                (Select  GROUP_CONCAT(IFNULL(ps.GeneratedVisitCode, 'N/A'))
                from Recertifications rr
                INNER JOIN
                PatientSchedule ps ON ps.RecertId = rr.Id
                WHERE
                rr.PatientId = r.PatientId AND ps.ClinicianId = {currentLoggedUser}),'N/A') AS Frequency,

              CONCAT_WS(' ', u.FirstName, u.LastName ) as NurseName")
                .From("Recertifications r");
            cmd.LeftJoin("Users u").On("u.Id = r.AddedBy");
            cmd.Where("r.Id=@0", id);
            if (patientid > 0)
                cmd.Where("r.PatientId=@0", patientid);

            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<RecertificationResponse>(cmd);
                return result;
            }
        }

        public RecertificationResponse GetNextRecertification(int patientId)
        {
            var cmd = Sql.Builder.Select($@"r.*,GROUP_CONCAT(IFNULL(ps.GeneratedVisitCode, 'N/A')) as Frequency,
            CONCAT_WS(' ', u.FirstName, u.LastName ) as NurseName,
            CONCAT(
                 CAST(AES_DECRYPT(UNHEX(p.FirstName),'{Encryption.Key}') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(p.LastName),'{Encryption.Key}') AS CHAR (150))
                 ) AS PatientName")
                .From("Recertifications r");
            cmd.LeftJoin("PatientSchedule ps").On("ps.RecertId = r.Id");
            cmd.LeftJoin("Users u").On("u.Id = r.AddedBy");
            cmd.LeftJoin("PatientProfiles p").On("r.PatientId = p.Id");
            cmd.Where("r.PatientId=@0", patientId);
            cmd.GroupBy("r.Id");
            cmd.OrderBy("r.RecertificationDate DESC");
            cmd.Append("LIMIT 1");

            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<RecertificationResponse>(cmd) ?? new RecertificationResponse();
                return result;
            }
        }

        public object SaveRecertification(Recertification recertification, int meId)
        {
            int recertid = recertification.Id;

            recertification.AddedOn = DateTime.UtcNow;
            recertification.AddedBy = meId;
            recertification.LastModOn = DateTime.UtcNow;
            var result = Common.Instances.RecertificationInst.Save(recertification, meId);

            if (recertid == 0)
            {
                var patient = Common.Instances.PatientProfileInst.Get(recertification.PatientId, meId);
                var date = new Dictionary<DateTime, string>
                {
                    {recertification.RecertificationDate.AddDays(29).AddSeconds(1), "30DRE"},
                    {recertification.RecertificationDate.AddDays(59).AddSeconds(2), "Recert"},
                    {recertification.RecertificationDate.AddDays(59).AddSeconds(3), "Eoc"},
                };
                foreach (var kvp in date)
                {
                    var Generatedate = new PatientDate
                    {
                        PatientId = patient.Id,
                        ClinicianId = meId,
                        RecertId = recertification.Id,
                        Status = "N/A",
                        PatientDates = kvp.Key,
                        Type = kvp.Value
                    };
                    Common.Instances.PatientProfileInst.PatientDateSave(Generatedate);
                }
            }
            return result;
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                const string cmd = @"Delete FROM Recertifications where Id=@0;
                                     Delete FROM PatientSchedule where RecertId=@0;
                                     Delete FROM PatientDates where RecertId=@0";
                var result = db.Execute(cmd, Id);
                return Convert.ToInt32(result);
            }
        }
    }
}