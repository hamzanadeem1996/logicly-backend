using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Response
{
    public class PatientAvailabilityResponse
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DayOfWeek WeekDayNo { get; set; }

        public string WeekDayName =>
            CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(WeekDayNo);

        public string StartHour { get; set; }
        public string EndHour { get; set; }

        public string Time
            => $"{Convert.ToDateTime(StartHour).ToString("h:mm tt")} - {Convert.ToDateTime(EndHour).ToString("h:mm tt")}";

        public bool IsAvailable { get; set; }
    }
}