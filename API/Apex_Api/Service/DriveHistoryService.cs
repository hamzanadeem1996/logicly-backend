using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using System;
using System.Collections.Generic;

namespace Apex_Api.Service
{
    public class DriveHistoryService
    {
        public object SaveDriveHistory(DateTime date, int clinicianId,
            decimal mile, int drivenTime, int currentLoggedUser)
        {
            var driveHistory = new DriveHistory
            {
                DriveDate = date,
                UserId = clinicianId,
                MilesDriven = mile,
                DrivenTime = drivenTime,
            };
            var saveDriveHistory = DriveHistoryRepo.Save(driveHistory, currentLoggedUser);
            return saveDriveHistory;
        }

        public void SaveDriveHistoryMultiple(IEnumerable<PatientVisitScheduleGet> data, int currentLoggedUser)
        {
            foreach (var item in data)
            {
                SaveDriveHistory(Convert.ToDateTime(item.Start.ToString("yyyy-MM-dd"))
                    , item.NurseId, item.DistanceValue, item.DurationMins, currentLoggedUser);
            }
        }
    }
}