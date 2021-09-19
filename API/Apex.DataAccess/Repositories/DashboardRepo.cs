using Apex.DataAccess.Response;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class DashboardRepo
    {
        public AgencyDashboardResponse GetAgencyDashboard(int agencyId, string date)
        {
            var cmd = Sql.Builder.Select($@"Count(u.Id) AS Clinicians,

             (SELECT Count(pvs.Id) FROM PatientVisitSchedule pvs
             INNER JOIN Users uu ON uu.Id = pvs.NurseId
             WHERE uu.AgencyId = u.AgencyId) AS VisitsScheduled,

             (SELECT Count(pd.Id) FROM PatientDates pd
             INNER JOIN PatientProfiles pp ON pp.Id = pd.PatientId
             WHERE pd.Status='MISSED' AND pp.AgencyId = u.AgencyId) AS MissedVisits,

            (SELECT Count(pvs1.Id) FROM PatientVisitSchedule pvs1
            INNER JOIN Users uuu ON uuu.Id = pvs1.NurseId
            WHERE uuu.AgencyId = u.AgencyId AND pvs1.Start BETWEEN CURDATE() - INTERVAL 30 DAY AND CURDATE()) AS VisitsLast30Days

            ").From("Users u");
            cmd.InnerJoin("Roles r").On("r.Id=u.RoleId");
            cmd.Where("u.AgencyId=@0 AND r.Name!=@1 ", agencyId, "SUPERADMIN");
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<AgencyDashboardResponse>(cmd);
                result.AdmissionCompleted = Monthly(agencyId, date);
                result.Weekly = Week(agencyId, date);
                result.Daily = Daily(agencyId, date);
                return result;
            }
        }

        public List<AdmissionsByMonth> Monthly(int agencyId, string date)
        {
            var cmd = Sql.Builder.Select($@"YEAR(Admission) as Year , MONTHNAME(Admission) as Month , COUNT(pp.Id) AS Count")
                .From("PatientProfiles pp");
            cmd.Where("pp.AgencyId=@0 AND YEAR(Admission) = @1", agencyId, Convert.ToDateTime(date).ToString("yyyy"));
            cmd.GroupBy("YEAR(Admission), MONTHNAME(Admission)");
            cmd.OrderBy("YEAR(Admission) DESC, MAX(MONTH(Admission)) ASC");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<AdmissionsByMonth>(cmd);
                return result;
            }
        }

        public List<Week> Week(int agencyId, string date)
        {        
            var cmd = Sql.Builder.Select($@"MAX(pp.Admission) as Admission, DAYNAME(Admission) as Day , COUNT(pp.Id) AS Count")
                .From("PatientProfiles pp");
            cmd.Where("pp.AgencyId = @0 AND pp.Admission BETWEEN @1 AND DATE_ADD(@1,INTERVAL 7 DAY)", agencyId, date);
            cmd.GroupBy("DAYNAME(Admission)");
            cmd.OrderBy("MAX(pp.Admission) ASC");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<Week>(cmd);
                return result;
            }
        }

        public Daily Daily(int agencyId, string date)
        {
            var cmd = Sql.Builder.Select($@"MAX(pp.Admission) as Admission, COUNT(pp.Id) AS Count")
                .From("PatientProfiles pp");
            cmd.Where("CAST(pp.Admission AS DATE) = @0 AND pp.AgencyId=@1 ", date, agencyId);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<Daily>(cmd);
                return result;
            }
        }

        public ClinicianDashboardResponse GetClinicianDashboard(int clinician, int agencyId)
        {
            var cmd = Sql.Builder.Select($@"
             Count(pp.Id) as Admissions,
             (SELECT Count(p.Id) FROM PatientProfiles p
             WHERE 
            p.OT = {clinician} OR p.PT=  {clinician} OR p.SLP = {clinician} OR p.SN =  {clinician} OR p.AID = {clinician} OR p.MSW = {clinician} OR
            p.AddedBy = {clinician} OR p.UserId=  {clinician} ) AS Patients,
            (select ROUND((SUM(DrivenTime)/60),2)  from DriveHistory WHERE UserId = pp.AddedBy) AS DrivingTime,
            (select ROUND((SUM(MilesDriven)/1.609),2)  from DriveHistory WHERE UserId = pp.AddedBy) AS LengthOfVisits
            ").From("PatientProfiles pp");

            cmd.Where("pp.AddedBy=@0 AND pp.AgencyId", clinician, agencyId);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<ClinicianDashboardResponse>(cmd);
                return result;
            }
        }

        public DrivenHistoryResponse GetDrivenHistory(int agencyId, int clinician, string date)
        {
            var cmd = Sql.Builder.Select($@"ROUND((SUM(MilesDriven)/1.609),2) AS Daily,
              MAX(dh.DriveDate) as DriveDate").From("DriveHistory dh");
            cmd.InnerJoin("Users u").On("u.Id = dh.UserId");
            if (clinician > 0) { cmd.Where("dh.UserId = @0", clinician); }
            cmd.Where("CAST(dh.DriveDate AS DATE) = @0 AND u.AgencyId=@1 ", date, agencyId);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<DrivenHistoryResponse>(cmd);
                result.DriveDate = Convert.ToDateTime(date);
                result.Month = DrivenMonth(agencyId, clinician, date);
                result.Week = DrivenWeek(agencyId, clinician, date);
                return result;
            }
        }

        public List<DrivenMonthly> DrivenMonth(int agencyId, int clinician, string date)
        {
            var cmd = Sql.Builder.Select($@"YEAR(dh.DriveDate) as Year , MONTHNAME(dh.DriveDate) as Month,
                 ROUND((SUM(dh.MilesDriven)/1.609),2) AS Distance")
                .From("DriveHistory dh");
            cmd.InnerJoin("Users u").On("u.Id = dh.UserId");
            cmd.Where("YEAR(dh.DriveDate) = @0 AND u.AgencyId=@1", Convert.ToDateTime(date).ToString("yyyy"), agencyId);
            if (clinician > 0) { cmd.Where("dh.UserId = @0", clinician); }
            cmd.GroupBy("YEAR(dh.DriveDate), MONTHNAME(dh.DriveDate)");
            cmd.OrderBy("YEAR(dh.DriveDate) DESC, MAX(MONTH(dh.DriveDate)) ASC");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<DrivenMonthly>(cmd);
                return result;
            }
        }

        public List<DrivenWeekly> DrivenWeek(int agencyId, int clinician, string date)
        {
            var d = Utility.GetNextWeekday(Convert.ToDateTime(date), DayOfWeek.Sunday).AddDays(-7);
            date = d.ToString("yyyy-MM-dd");

            var cmd = Sql.Builder.Select($@"MAX(dh.DriveDate) as DriveDate, DAYNAME(dh.DriveDate) as Day,
                      ROUND((SUM(dh.MilesDriven)/1.609),2) AS Distance")
                .From("DriveHistory dh");
            cmd.InnerJoin("Users u").On("u.Id = dh.UserId");
            if (clinician > 0) { cmd.Where("dh.UserId = @0", clinician); }
            cmd.Where("dh.DriveDate BETWEEN @0 AND DATE_ADD(@0,INTERVAL 6 DAY) AND u.AgencyId = @1", date, agencyId);
            cmd.GroupBy("DAYNAME(dh.DriveDate)");
            cmd.OrderBy("MAX(dh.DriveDate) ASC");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<DrivenWeekly>(cmd);
                return result;
            }
        }
    }
}