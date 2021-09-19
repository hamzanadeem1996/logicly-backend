using System;

namespace Apex.DataAccess.Response
{
    public class AgencyDashboardResponse
    {
        public int VisitsScheduled { get; set; }
        public int Clinicians { get; set; }
        public int MissedVisits { get; set; }
        public int VisitsLast30Days { get; set; }
        public object AdmissionCompleted { get; set; }
        public object Weekly { get; set; }
        public object Daily { get; set; }
    }

    public class AdmissionsByMonth
    {
        public int Year { get; set; }
        public string Month { get; set; }
        public int Count { get; set; }
    }

    public class Week
    {
        public DateTime Admission { get; set; }
        public string Day { get; set; }
        public int Count { get; set; }
    }

    public class Daily
    {
        public DateTime Admission { get; set; }
        public int Count { get; set; }
    }

    public class DrivenHistoryResponse
    {
        public object Month { get; set; }
        public object Week { get; set; }

        public DateTime DriveDate { get; set; }
        public string Daily { get; set; }

        public string Unit => "Miles";
    }

    public class DrivenMonthly
    {
        public int Year { get; set; }
        public string Month { get; set; }
        public string Distance { get; set; }
    }

    public class DrivenWeekly
    {
        public DateTime DriveDate { get; set; }
        public string Day { get; set; }
        public string Distance { get; set; }
    }
}