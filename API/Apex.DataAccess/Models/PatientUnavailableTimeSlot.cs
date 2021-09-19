using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("PatientUnavailableTimeSlots")]
    [PrimaryKey("Id")]
    public class PatientUnavailableTimeSlot
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string SlotDetail { get; set; }
        public int AddedBy { get; set; }
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }
    }
}