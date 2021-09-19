using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class PatientVisitScheduleRepo
    {
        public PatientDate GetPatientDate(string startdate, int patientId, string type, int clinicianId)
        {
            var cmd = Sql.Builder.Select("*").From("PatientDates");
            cmd.Where("PatientId=@0 AND Type=@1 AND ClinicianId=@2", patientId, type, clinicianId);
            cmd.Where("cast(PatientDates as date) = @0", startdate);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientDate>(cmd) ?? new PatientDate();
                return result;
            }
        }

        public PatientVisitSchedule Get(int patientid)
        {
            var cmd = Sql.Builder.Select($@"pv.*,
                CAST(AES_DECRYPT(UNHEX(pp.Address),'{Encryption.Key}') AS CHAR (250)) AS PatientAddress,
                 CONCAT(
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(pp.LastName),'{Encryption.Key}') AS CHAR (150))
                 ) AS PatientName")
                .From("PatientVisitSchedule pv");
            cmd.LeftJoin("PatientProfiles pp").On("pv.PatientId = pp.Id");
            cmd.Where("pv.PatientId=@0", patientid);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientVisitSchedule>(cmd);
                return result;
            }
        }

        public PatientVisitSchedule GetVisitSchedule(int id)
        {
            var cmd = Sql.Builder.Select("*").From("PatientVisitSchedule");
            cmd.Where("Id=@0", id);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientVisitSchedule>(cmd) ?? new PatientVisitSchedule();
                return result;
            }
        }


        public List<PatientVisitSchedule> GetCombinationVisit(string CombinationVisit)
        {
            var cmd = Sql.Builder.Select("*").From("PatientVisitSchedule");
            cmd.Where("CombinationVisit=@0", CombinationVisit);
            using (var db = Utility.Database)
            {
                var result = db.Fetch<PatientVisitSchedule>(cmd);
                return result;
            }
        }



        public Page<PatientVisitSchedule> GetAll(int pagenumber, int pagesize, string query = "", int userid = 0,
            int patientid = 0, int addedBy = 0)
        {
            var cmd = Sql.Builder.Select($@"p.*,
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150)) AS PatientName,
                 CAST(AES_DECRYPT(UNHEX(pp.Address),'{Encryption.Key}') AS CHAR (250)) AS PatientAddress,
                 CONCAT(
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(pp.LastName),'{Encryption.Key}') AS CHAR (150))
                 ) AS Title,

                 (SELECT EXISTS(SELECT * FROM PatientVisitSchedule pvs INNER JOIN
                PatientDates pd  ON pd.PatientId = pvs.PatientId
                WHERE
                pd.PatientId = pvs.PatientId
                AND pd.IsAddedToSchedule = 1 AND pd.Status='COMPLETED'
                AND date(pd.PatientDates) = date(pvs.RoutineVisitDate)
                AND pvs.PatientId = p.PatientId)) AS IsCompleted,

              u.Lat as NurseLat,u.Long as NurseLong,pp.Lat as PatientLat ,pp.Long as PatientLong,IFNULL(s.Units, 'N/A') as Units")
                .From("PatientVisitSchedule p");

            cmd.LeftJoin("Settings s").On("p.NurseId = s.UserId");
            cmd.LeftJoin("PatientProfiles pp").On("p.PatientId = pp.Id");
            cmd.LeftJoin("Users u").On("p.NurseId = u.Id");

            if (userid > 0)
            {
                cmd.Where("p.NurseId=@0 OR p.colorType='R'", userid);
            }

            if (patientid > 0)
            {
                cmd.Where("p.PatientId=@0", patientid);
            }

            if (addedBy > 0)
            {
                cmd.Where("p.AddedBy=@0", addedBy);
            }

            cmd.Where($"(pp.Lat != {0}) AND (pp.Long != {0}) AND (u.Lat != {0}) AND (u.Long != {0})");
            using (var db = Utility.Database)
            {
                var result = db.Page<PatientVisitSchedule>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public Page<PatientVisitSchedule> GetVisitScheduleByDate(string startdate, int pagenumber, int pagesize,
            string query = "", int userid = 0, int patientid = 0)
        {
            var cmd = Sql.Builder.Select($@"p.*,
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150)) AS PatientName,
                 CAST(AES_DECRYPT(UNHEX(pp.Address),'{Encryption.Key}') AS CHAR (250)) AS PatientAddress,
                 CONCAT(
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(pp.LastName),'{Encryption.Key}') AS CHAR (150))
                 ) AS Title,
                 (SELECT EXISTS(SELECT * FROM PatientVisitSchedule pvs INNER JOIN
                PatientDates pd  ON pd.PatientId = pvs.PatientId
                WHERE
                pd.PatientId = pvs.PatientId
                AND pd.IsAddedToSchedule = 1 AND pd.Status='COMPLETED'
                AND date(pd.PatientDates) = date(pvs.RoutineVisitDate)
                AND pvs.PatientId = p.PatientId)) AS IsCompleted,
                 u.Lat as NurseLat,u.Long as NurseLong,pp.Lat as PatientLat,
                 pp.Long as PatientLong,IFNULL(s.Units, 'N/A') as Units")
                .From("PatientVisitSchedule p");
            cmd.LeftJoin("Settings s").On("p.NurseId = s.UserId");
            cmd.LeftJoin("PatientProfiles pp").On("p.PatientId = pp.Id");
            cmd.LeftJoin("Users u").On("p.NurseId = u.Id");
            if (userid > 0)
            {
                cmd.Where("p.AddedBy=@0 OR p.NurseId=@0", userid);
                //cmd.Where("p.colorType='R'"); //ONLY GET REGULAR VISITS
            }

            if (patientid > 0) cmd.Where("p.PatientId=@0", patientid);

            if (startdate != "0001-01-01") cmd.Where("cast(p.Start as date) = @0", startdate);

            cmd.Where($"(pp.Lat != {0}) AND (pp.Long != {0}) AND (u.Lat != {0}) AND (u.Long != {0})");
            cmd.OrderBy("p.Start");
            using (var db = Utility.Database)
            {
                var result = db.Page<PatientVisitSchedule>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public Page<SinglePatientVisitSchedule> GetSingleVisitSchedule(int userid, int patientId, int pagenumber,
            int pagesize, string query = "")
        {
            var cmd = Sql.Builder
                .Select(
                    "p.*,Concat(u.FirstName,' ',u.LastName) as Title,pp.OT,pp.OTA,pp.PT,pp.PTA,pp.SLP,pp.SN,pp.AID,pp.MSW")
                .From("PatientVisitSchedule p");
            cmd.LeftJoin("PatientProfiles pp").On("p.PatientId = pp.Id");
            cmd.LeftJoin("Users u").On("p.NurseId = u.Id");
            cmd.Where("p.PatientId=@0 AND p.NurseId=@1", patientId, userid);
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<SinglePatientVisitSchedule>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object Save(PatientVisitSchedule patientVisitSchedule)
        {
            using (var db = Utility.Database)
            {
                if (patientVisitSchedule.Id == 0)
                {
                    patientVisitSchedule.AddedOn = DateTime.UtcNow;
                }

                patientVisitSchedule.LastModOn = DateTime.UtcNow;
                db.Save(patientVisitSchedule);
                return patientVisitSchedule;
            }
        }

        public int Delete(int id)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from PatientVisitSchedule");
                cmd.Where("Id=@0", id);
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }

        // public int UpdateVisitLockStatus(int patientId, bool isLocked)
        // {
        //     using (var db = Utility.Database)
        //     {
        //         var cmd = Sql.Builder.Append($"UPDATE VisitLockStatus SET IsLocked={isLocked} WHERE PatientId =@0", patientId);
        //         var result = db.Execute(cmd);
        //         return Convert.ToInt32(result);
        //     }
        // }

        public int DeleteByUserId(int id, string visitdate)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from PatientVisitSchedule");
                if (id > 0)
                {
                    cmd.Where("NurseId=@0 AND cast(Start as date) = @1", id, visitdate);
                }

                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }

        public PatientVisitSchedule CheckPatientSchdeuleExist(int userid, int patientId, DateTime date)
        {
            var cmd = Sql.Builder.Select($@"pv.*,
                CONCAT
                 (
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(pp.LastName),'{Encryption.Key}') AS CHAR (150))
                 ) AS Title").From("PatientVisitSchedule pv");
            cmd.LeftJoin("PatientProfiles pp").On("pv.PatientId = pp.Id");
            cmd.Where("pv.NurseId=@0", userid);
            cmd.Where("pv.PatientId=@0", patientId);
            cmd.Where("cast(pv.Start as date)=@0", date.Date);
            cmd.Where("cast(pv.End as date)=@0", date.Date);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientVisitSchedule>(cmd);
                return result;
            }
        }

        public PatientVisitSchedule CheckVisitExistInWeekend(int userid)
        {
            var cmd = Sql.Builder.Append($@"SELECT *, WEEKDAY (Start) as WEEKDAY
                                            FROM PatientVisitSchedule
                                            WHERE  WEEKDAY(Start) >= 5 AND NurseId={userid}
                                            AND Start >= NOW()");
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientVisitSchedule>(cmd);
                return result;
            }
        }

        public PatientVisitSchedule GetVisitByRoutineDate(string startdate, int patientid = 0)
        {
            var cmd = Sql.Builder.Select($@"*").From("PatientVisitSchedule");
            cmd.Where("PatientId=@0", patientid);
            cmd.Where("cast(RoutineVisitDate as date) = @0", startdate);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientVisitSchedule>(cmd);
                return result;
            }
        }

        public int DeletePatientVisitSchedule(int patientid, int recertId, string Colortype = "RV", int clinicianId = 0)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from PatientVisitSchedule");
                cmd.Where("PatientId=@0 AND RecertId=@1 AND NurseId=@2", patientid, recertId, clinicianId);
                cmd.Where("ColorType=@0 OR ColorType='D' OR ColorType='30' OR ColorType='E'", Colortype);
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }


        public int DeletePatientDischargeVisitSchedule(int patientid, int recertId,int clinicianId = 0)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from PatientVisitSchedule");
                cmd.Where("PatientId=@0 AND RecertId=@1 AND NurseId=@2", patientid, recertId, clinicianId);
                cmd.Where("ColorType='D'");
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }
    }
}