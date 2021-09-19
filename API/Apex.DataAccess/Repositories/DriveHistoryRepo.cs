using Apex.DataAccess.Models;
using NPoco;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class DriveHistoryRepo
    {
        public DriveHistory Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<DriveHistory>(id) ?? new DriveHistory();
                return result;
            }
        }

        public static object Save(DriveHistory driveHistory, int currentLoggedUser)
        {
            using (var db = Utility.Database)
            {
                if (driveHistory.Id == 0)
                {
                    driveHistory.AddedOn = DateTime.UtcNow;
                    driveHistory.AddedBy = currentLoggedUser;
                }

                driveHistory.LastModOn = DateTime.UtcNow;
                driveHistory.LastModBy = currentLoggedUser;

                db.Save(driveHistory);
                return driveHistory;
            }
        }

        public int Delete(int clinicianId, string date)
        {
            using (var db = Utility.Database)
            {
                var cmd = Sql.Builder.Append("Delete from DriveHistory");
                cmd.Where("UserId=@0 AND DriveDate=@1", clinicianId, date);
                var result = db.Execute(cmd);
                return Convert.ToInt32(result);
            }
        }

        public int DeleteMultiple(List<PatientVisitScheduleGet> schedule)
        {
            foreach (var sc in schedule)
            {
                new DriveHistoryRepo().Delete(sc.NurseId, sc.Start.ToString("yyyy-MM-dd"));
            }

            return schedule.Count;
        }
    }
}