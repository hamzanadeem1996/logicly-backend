using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.RequestModel
{
    public class PatientAvailabilityRequest
    {
        [JsonIgnore] public int Id { get; set; }
        public int PatientId { get; set; }
        public int[] WeekDayNo { get; set; }
        public string StartHour { get; set; } = "";
        public string EndHour { get; set; } = "";
        [DefaultValue(false)] public bool IsAvailable { get; set; }
    }
}