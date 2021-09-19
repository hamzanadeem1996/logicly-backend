using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Apex.DataAccess.Repositories
{
    public class PatientProfileRepo
    {
        public PatientProfile Get(int id, int currentLoggedUser)
        {
            var cmd = Sql.Builder.Select(
                    $@"p.Id,p.Admission,p.Notes,p.UserId,p.MDNumber,p.MDName,p.CareTeamId,p.TeamLeader,p.OT,p.OTA,p.PT,p.PTA,p.SLP,p.SN,p.AID,
                p.CityName,p.ZipCode,p.Lat,p.Long,
                p.MSW,p.Status,p.AgencyId,p.AddedBy,p.LastModBy,p.AddedOn,p.LastModOn,
                CAST(AES_DECRYPT(UNHEX(p.FirstName),'{Encryption.Key}') AS CHAR (150)) AS FirstName,
                CAST(AES_DECRYPT(UNHEX(p.LastName),'{Encryption.Key}') AS CHAR (150)) AS LastName,
                CAST(AES_DECRYPT(UNHEX(p.Address),'{Encryption.Key}') AS CHAR (150)) AS Address,
                CAST(AES_DECRYPT(UNHEX(p.PrimaryNumber),'{Encryption.Key}') AS CHAR (150)) AS PrimaryNumber,
                CAST(AES_DECRYPT(UNHEX(p.SecondaryNumber),'{Encryption.Key}') AS CHAR (150)) AS SecondaryNumber,
                CAST(AES_DECRYPT(UNHEX(p.PreferredName),'{Encryption.Key}') AS CHAR (150)) AS PreferredName,
                Concat(a.FirstName,' ',a.LastName) as OTNAME,
                Concat(b.FirstName,' ',b.LastName) as OTAName,Concat(c.FirstName,' ',c.LastName) as PTName,
                Concat(d.FirstName,' ',d.LastName) as PTAName,Concat(e.FirstName,' ',e.LastName) as SLPName,
                Concat(f.FirstName,' ',f.LastName) as SNName,Concat(g.FirstName,' ',g.LastName) as AIDName,
                Concat(j.FirstName,' ',j.LastName) as MSWName ,Concat(i.FirstName,' ',i.LastName) as TeamLeaderName,
                IFNULL(p.Status, 'N/A'),

                IFNULL(
                (Select  GROUP_CONCAT(IFNULL(ps1.GeneratedVisitCode, 'N/A')SEPARATOR ', ')
                from Recertifications r
                INNER JOIN
                PatientSchedule ps1 ON ps1.RecertId = r.Id
                WHERE
                r.PatientId = p.Id AND ps1.ClinicianId = {currentLoggedUser}
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1),'N/A') AS Frequency,

                (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AS ActiveCertId,

                (SELECT COUNT(Id) FROM Recertifications WHERE PatientId =  p.Id ) AS CertificationPeriodCount,

                (SELECT EXISTS(SELECT * FROM PatientDates pd
                WHERE pd.PatientId = p.Id
                AND pd.IsAddedToSchedule = 1 AND pd.Status='COMPLETED'
                AND pd.Type = 'Evaluation')) AS EvalCompleted,

                (SELECT PatientDates  FROM PatientDates
                WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id  LIMIT 1)
                AND ClinicianId= {currentLoggedUser} AND Type = 'Evaluation' LIMIT 1) AS Evaluation,

                IFNULL((SELECT MAX(PatientDates)  FROM PatientDates
                WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id  LIMIT 1)
                AND ClinicianId= {currentLoggedUser} AND Type = '30DRE' AND Status='COMPLETED' LIMIT 1),'N/A') AS Most30DRE,


                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND ClinicianId= {currentLoggedUser} AND Type = '30DRE' LIMIT 1) AS ThirtyDaysRelEval,

                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId= (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND ClinicianId= {currentLoggedUser}
                AND Type = 'Discharge' LIMIT 1) AS Discharge,

                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId= (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND Type = 'Recert' LIMIT 1) AS Recert,

                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND Type = 'Eoc' LIMIT 1) AS Eoc,

                IFNULL(
                (SELECT DATE_FORMAT(RecertificationDate, '%b %d, %Y')  FROM Recertifications
                WHERE PatientId = p.Id
                AND
                ( RecertificationDate BETWEEN  DATE_ADD(CURDATE(), INTERVAL 59 DAY) AND CURDATE()
                OR
                RecertificationDate BETWEEN  DATE_SUB(CURDATE(), INTERVAL 59 DAY) AND CURDATE() )
                order BY RecertificationDate DESC LIMIT 1),'N/A') AS CurrentCertPeriod")
                .From("PatientProfiles p");

            cmd.LeftJoin("Users a").On("p.OT=a.Id");
            cmd.LeftJoin("Users b").On("p.OTA=b.Id");
            cmd.LeftJoin("Users c").On("p.PT=c.Id");
            cmd.LeftJoin("Users d").On("p.PTA=d.Id");
            cmd.LeftJoin("Users e").On("p.SLP=e.Id");
            cmd.LeftJoin("Users f").On("p.SN=f.Id");
            cmd.LeftJoin("Users g").On("p.AID=g.Id");
            cmd.LeftJoin("Users i").On("p.TeamLeader=i.Id");
            cmd.LeftJoin("Users j").On("p.MSW=j.Id");

            cmd.LeftJoin("PatientSchedule ps").On("ps.PatientId = p.Id");
            cmd.Where("p.Id=@0", id);
            cmd.GroupBy("p.Id");
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientProfile>(cmd);

                return result;
            }
        }

        public Page<PatientProfile> GetAll(List<int> ids, List<int> userid, int pagenumber, int pagesize,
            string query = "",
            string status = "", string filter = "validaddress", int agencyId = 0, bool isAdmin = false)
        {
            var cmd = Sql.Builder.Select(
                    $@"p.Id,p.Admission,p.Notes,p.UserId,p.MDNumber,p.MDName,p.CareTeamId,p.TeamLeader,p.OT,p.OTA,p.PT,p.PTA,p.SLP,p.SN,p.AID,
                p.CityName,p.ZipCode,p.Lat,p.Long,
                p.MSW,p.Status,p.AgencyId,p.AddedBy,p.LastModBy,p.AddedOn,p.LastModOn,
                CAST(AES_DECRYPT(UNHEX(p.FirstName),'{Encryption.Key}') AS CHAR (150)) AS FirstName,
                CAST(AES_DECRYPT(UNHEX(p.LastName),'{Encryption.Key}') AS CHAR (150)) AS LastName,
                CAST(AES_DECRYPT(UNHEX(p.Address),'{Encryption.Key}') AS CHAR (150)) AS Address,
                CAST(AES_DECRYPT(UNHEX(p.PrimaryNumber),'{Encryption.Key}') AS CHAR (150)) AS PrimaryNumber,
                CAST(AES_DECRYPT(UNHEX(p.SecondaryNumber),'{Encryption.Key}') AS CHAR (150)) AS SecondaryNumber,
                CAST(AES_DECRYPT(UNHEX(p.PreferredName),'{Encryption.Key}') AS CHAR (150)) AS PreferredName,
                Concat(a.FirstName,' ',a.LastName) as OTNAME,
                Concat(b.FirstName,' ',b.LastName) as OTAName,Concat(c.FirstName,' ',c.LastName) as PTName,
                Concat(d.FirstName,' ',d.LastName) as PTAName,Concat(e.FirstName,' ',e.LastName) as SLPName,
                Concat(f.FirstName,' ',f.LastName) as SNName,Concat(g.FirstName,' ',g.LastName) as AIDName,
                Concat(j.FirstName,' ',j.LastName) as MSWName ,Concat(i.FirstName,' ',i.LastName) as TeamLeaderName,
                IFNULL(p.Status, 'N/A'),

                IFNULL(
                (Select  GROUP_CONCAT(IFNULL(ps1.GeneratedVisitCode, 'N/A')SEPARATOR ', ')
                from Recertifications r
                INNER JOIN
                PatientSchedule ps1 ON ps1.RecertId = r.Id
                WHERE
                r.PatientId = p.Id AND ps1.ClinicianId = {userid[0]}
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1),'N/A') AS Frequency,

                (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AS ActiveCertId,

                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id  LIMIT 1)
                 AND ClinicianId= {userid[0]}   AND Type = 'Evaluation' LIMIT 1) AS Evaluation,

                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND ClinicianId= {userid[0]}   AND Type = '30DRE' LIMIT 1) AS ThirtyDaysRelEval,

                 IFNULL((SELECT MAX(PatientDates)  FROM PatientDates
                WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id  LIMIT 1)
                AND ClinicianId=  {userid[0]} AND Type = '30DRE' AND Status='COMPLETED' LIMIT 1),'N/A') AS Most30DRE,


                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId =  (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND ClinicianId = {userid[0]}
                 AND Type = 'Discharge' LIMIT 1) AS Discharge,

                (SELECT PatientDates  FROM PatientDates
                 WHERE PatientId = p.Id  AND RecertId= (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND Type = 'Recert' LIMIT 1) AS Recert,

                (SELECT PatientDates  FROM PatientDates
                WHERE PatientId = p.Id  AND RecertId = (SELECT Id FROM Recertifications r WHERE r.PatientId = p.Id
                AND
                (CAST(r.RecertificationDate AS DATE) BETWEEN CURDATE() AND  (DATE_ADD(CURDATE(), INTERVAL 59 DAY))
                OR
                CAST(r.RecertificationDate AS DATE) BETWEEN  (DATE_SUB(CURDATE(), INTERVAL 59 DAY)) AND CURDATE())
                LIMIT 1) AND Type = 'Eoc' LIMIT 1) AS Eoc,

                (SELECT COUNT(Id) FROM Recertifications WHERE PatientId =  p.Id ) AS CertificationPeriodCount,

                IFNULL(
                (SELECT DATE_FORMAT(RecertificationDate, '%b %d, %Y')  FROM Recertifications
                WHERE PatientId = p.Id
                AND
                ( RecertificationDate BETWEEN  DATE_ADD(CURDATE(), INTERVAL 59 DAY) AND CURDATE()
                OR
                RecertificationDate BETWEEN  DATE_SUB(CURDATE(), INTERVAL 59 DAY) AND CURDATE() )
                order BY RecertificationDate DESC LIMIT 1),'N/A') AS CurrentCertPeriod")
                .From("PatientProfiles p");

            cmd.LeftJoin("Users a").On("p.OT=a.Id");
            cmd.LeftJoin("Users b").On("p.OTA=b.Id");
            cmd.LeftJoin("Users c").On("p.PT=c.Id");
            cmd.LeftJoin("Users d").On("p.PTA=d.Id");
            cmd.LeftJoin("Users e").On("p.SLP=e.Id");
            cmd.LeftJoin("Users f").On("p.SN=f.Id");
            cmd.LeftJoin("Users g").On("p.AID=g.Id");
            cmd.LeftJoin("Users i").On("p.TeamLeader=i.Id");
            cmd.LeftJoin("Users j").On("p.MSW=j.Id");
            cmd.LeftJoin("PatientSchedule ps").On("ps.PatientId = p.Id");

            if (ids.Count > 0)
            {
                cmd.Where("p.Id IN (@0)", ids);
            }

            cmd.Where("p.AgencyId =@0", agencyId);
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Where("LOWER(p.Status=@0)", status.ToLower());
            }

            //N0te:-Clinicians will only see frequencies for patients if they are seeing that patient,
            //    if they have been selected in OT, PT etc, any of these fields

            //Note:-  On clinician linked to a patient will be able to see that patient When a patient(Sally) is added,
            //   the user would say pick John and Joe are their therapists.John and Joe are the only clinicians
            //     who can see this patient(sally) in the app and admin.Anyone else should not even see this
            //     patients name. Sally is invisible to EVERYONE, except the clinicians assigned to them: John and
            //     Joe.We don’t need to see blanked out frequency because the patient is invisible to everyone except
            //     the clinicians assigned.

            if (userid.Count > 0 && !isAdmin)
            {
                cmd.Where($@"p.OT IN (@0) OR p.OTA IN (@0) OR p.PT IN (@0) OR p.PTA IN (@0) OR p.SLP IN (@0)
                OR p.SN IN (@0) OR p.AID IN (@0) OR p.MSW IN (@0) OR p.AddedBy IN (@0) OR p.UserId IN (@0) OR p.TeamLeader IN (@0)",
                    userid);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                cmd.Where($@"
                 CAST(AES_DECRYPT(UNHEX(p.FirstName),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                 CAST(AES_DECRYPT(UNHEX(p.LastName),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                 CAST(AES_DECRYPT(UNHEX(p.PreferredName),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                 CAST(AES_DECRYPT(UNHEX(p.Address),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                 CAST(AES_DECRYPT(UNHEX(p.PrimaryNumber),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                 CAST(AES_DECRYPT(UNHEX(p.SecondaryNumber),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                 CAST(AES_DECRYPT(UNHEX(p.CityName),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                 CAST(AES_DECRYPT(UNHEX(p.MDName),'{Encryption.Key}') AS CHAR (150)) LIKE @0 OR
                   p.UserId like @0", $"%{query}%");
            }

            cmd.GroupBy("p.Id");
            cmd.OrderBy("p.FirstName");

            using (var db = Utility.Database)
            {
                var result = db.Page<PatientProfile>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public PatientProfile Save(PatientProfile patientProfile, int currentUserId)
        {
            using (var db = Utility.Database)
            {
                if (patientProfile.Id == 0) patientProfile.AddedOn = DateTime.UtcNow;

                patientProfile.FirstName = patientProfile.FirstName.Encrypt();
                patientProfile.LastName = patientProfile.LastName.Encrypt();
                patientProfile.Address = patientProfile.Address.Encrypt();
                patientProfile.PreferredName = patientProfile.PreferredName.Encrypt();
                patientProfile.PrimaryNumber = patientProfile.PrimaryNumber.Encrypt();
                patientProfile.SecondaryNumber = patientProfile.SecondaryNumber.Encrypt();
                
                patientProfile.LastModOn = DateTime.UtcNow;
                db.Save(patientProfile);
                patientProfile = Get(patientProfile.Id, currentUserId);
                return patientProfile;
            }
        }

        public List<PatientDate> PatientDates(List<int> userid, string start, string end, int patientid,
            bool filterByDate = false,
            string status = "Discharged", int agencyId = 0)
        {
            var cmd = Sql.Builder.Select($@"pd.*,
             pp.FirstName as PatientFirstName,pp.LastName as PatientLastName , r.RecertificationDate as CertStartDate")
                .From("PatientDates pd");
            cmd.InnerJoin("PatientProfiles pp").On("pd.PatientId = pp.Id");
            cmd.LeftJoin("Recertifications r").On("r.Id = pd.RecertId");

            if ((patientid == 0 || filterByDate) && start != "0001-01-01")
            {
                cmd.Where("cast(pd.PatientDates as date) >= @0 AND cast(pd.PatientDates as date) <= @1", start, end);
            }

            if (patientid > 0)
                cmd.Where("pd.PatientId=@0", patientid);
            else
                cmd.Where("pp.AgencyId=@0", agencyId);

            if (patientid > 0 && start != "0001-01-01" && !filterByDate)
                cmd.Where("cast(pd.PatientDates as date) >= @0", start);

            cmd.Where("LOWER(pp.Status!=@0)", status.ToLower());
            cmd.Where("pd.Status != 'HIDE' OR pd.Status IS NULL");
            cmd.Where("pd.ClinicianId = 0 OR pd.ClinicianId IN (@0) OR pd.Type='Recert'", userid);
            cmd.OrderBy("pd.PatientDates");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<PatientDate>(cmd);
                return result;
            }
        }

        public List<PatientCsvResponse> GetAllPatientforCsv()
        {
            var cmd = Sql.Builder.Select($@"
                CAST(AES_DECRYPT(UNHEX(p.FirstName),'{Encryption.Key}') AS CHAR (150)) AS FirstName,
                CAST(AES_DECRYPT(UNHEX(p.PrimaryNumber),'{Encryption.Key}') AS CHAR (150)) AS PrimaryNumber,p.*")
                .From("PatientProfiles p");
            cmd.OrderBy("p.FirstName");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<PatientCsvResponse>(cmd);
                return result;
            }
        }

        public List<PatientProfile> GetAllPatient(List<int> ClinicianId, string OTA = "", string PTA = "",
            string AID = "")
        {
            var cmd = Sql.Builder.Select($@"
                CAST(AES_DECRYPT(UNHEX(p.FirstName),'{Encryption.Key}') AS CHAR (150)) AS FirstName,
                CAST(AES_DECRYPT(UNHEX(p.PrimaryNumber),'{Encryption.Key}') AS CHAR (150)) AS PrimaryNumber,p.*")
                .From("PatientProfiles p");
            if (!string.IsNullOrWhiteSpace(OTA))
            {
                cmd.Where("p.OTA IN (@0)", ClinicianId);
            }

            if (!string.IsNullOrWhiteSpace(PTA))
            {
                cmd.Where("p.PTA IN (@0)", ClinicianId);
            }

            if (!string.IsNullOrWhiteSpace(AID))
            {
                cmd.Where("p.AID IN (@0)", ClinicianId);
            }

            cmd.OrderBy("p.FirstName");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<PatientProfile>(cmd);
                return result;
            }
        }

        public object PatientDateSave(PatientDate patientDate)
        {
            using (var db = Utility.Database)
            {
                db.Save(patientDate);
                return patientDate;
            }
        }

        public int DeleteByPatientid(int id, string excludeVisitType = "RoutineVisit", List<int> assigneeIds = null)
        {
            if (assigneeIds == null || assigneeIds.Count<=0) return 0;
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append($"Delete from PatientDates");
                cmd.Where("PatientId IN (@0)", id);
                var strIds = string.Join(",", assigneeIds);
                cmd.Where($"Type!=@0 AND ClinicianId IN({strIds})", excludeVisitType);
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                const string cmd = @"Delete from PatientProfiles where Id=@0;
                                     Delete FROM PatientDates where PatientId=@0;
                                     Delete FROM PatientSchedule where PatientId =@0;
                                     Delete FROM PatientVisitSchedule where PatientId =@0";
                var result = db.Execute(cmd, Id);
                return Convert.ToInt32(result);
            }
        }

        public int UpdateEvaluationDate(int userid, int patientId, int recertid, DateTime evalDate, string freq = "",
            string eval = "Evaluation",
            string thirtyDaysRelEval = "30DRE", string dischargetype = "Discharge")
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append($@"
                            UPDATE PatientDates
                            SET
                                PatientDates = '{evalDate.ToString("yyyy-MM-dd HH:mm:ss")}'
                            WHERE
                                PatientId = {patientId} AND RecertId = {recertid} AND ClinicianId = {userid}
                            AND
                                Type = '{eval}';

                            UPDATE PatientDates
                            SET
                                 PatientDates = '{evalDate.AddDays(29).ToString("yyyy-MM-dd HH:mm:ss")}'
                            WHERE
                                PatientId = {patientId} AND RecertId = {recertid} AND ClinicianId = {userid}
                            AND
                                Type ='{thirtyDaysRelEval}'");

                var result = db.Execute(cmd);

                DateTime disCharge = new DateTime();
                if (freq == "1w1" && patientId > 0)
                {
                    disCharge = Utility.GetNextWeekday(evalDate, DayOfWeek.Saturday);
                    var cmd1 = Sql.Builder.Append($@"
                            UPDATE PatientDates
                            SET
                                PatientDates = '{disCharge.ToString("yyyy-MM-dd HH:mm:ss")}'
                            WHERE
                                PatientId = {patientId} AND RecertId = {recertid} AND ClinicianId = {userid}
                            AND
                                Type = '{dischargetype}'");
                    db.Execute(cmd1);
                }

                return Convert.ToInt32(result);
            }
        }

        public object SaveDischargeDate(int patientId, int recertId, int clinicianId,
            DateTime dischargeDate, string discharge = "Discharge", string status = "",string frequencytype="")
        {
            var GetDisCharge = new PatientDateRepo().GetPatientDatesByType(patientId, clinicianId, recertId, discharge);
            if (GetDisCharge.Id > 0)
            {
                GetDisCharge.PatientDates = dischargeDate;
                GetDisCharge.Status = GetDisCharge.Status.ToUpper() == "COMPLETED" ? GetDisCharge.Status : status;
                GetDisCharge.IsAddedToSchedule = false;
                GetDisCharge.FrequencyType = frequencytype;
            }
            else
            {
                GetDisCharge.PatientId = patientId;
                GetDisCharge.RecertId = recertId;
                GetDisCharge.ClinicianId = clinicianId;
                GetDisCharge.Type = discharge;
                GetDisCharge.PatientDates = dischargeDate;
                GetDisCharge.Status = status;
                GetDisCharge.IsAddedToSchedule = false;
                GetDisCharge.FrequencyType = frequencytype;
            }

            new PatientDateRepo().Save(GetDisCharge);
            return GetDisCharge;
        }
    }
}