using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("ClinicianAvailability")]
    [PrimaryKey("Id")]
    public class ClinicianAvailability
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int ClinicianId { get; set; }
        public string WeekDayNo { get; set; }
        public DateTime StartHour { get; set; }
        public DateTime EndHour { get; set; }
        public bool IsAvailable { get; set; }
    }
}