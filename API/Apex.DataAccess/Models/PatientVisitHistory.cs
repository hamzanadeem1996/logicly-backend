using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("PatientVisitHistory")]
    [PrimaryKey("Id")]
    public class PatientVisitHistory
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public DateTime VisitDate { get; set; }
        public DateTime DischargeDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public int AddedBy { get; set; }
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }
    }
}