using NPoco;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("PatientSchedule")]
    [PrimaryKey("Id")]
    public class PatientSchedule
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public int ClinicianId { get; set; }

        [JsonIgnore]
        public int RecertId { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string PatientName { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string ClinicianName { get; set; }

        [JsonIgnore]
        public int NumberOfUnits { get; set; }

        [JsonIgnore]
        public int VisitsPerUnit { get; set; }

        public string GeneratedVisitCode { get; set; }

        [JsonIgnore]
        public string Unit { get; set; }

        [JsonIgnore]
        public int SortIndx { get; set; }
    }

    public class PatientScheduleSave
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore] public int ClinicianId { get; set; }
        public int PatientId { get; set; }
        public int RecertId { get; set; }

        [JsonIgnore]
        public int NumberOfUnits { get; set; }

        [JsonIgnore]
        public int VisitsPerUnit { get; set; }

        public string[] GeneratedVisitCode { get; set; }

        [JsonIgnore]
        public string Unit { get; set; }

        [JsonIgnore]
        public int SortIndx { get; set; }

        [ResultColumn]
        public bool IsFrequencyNew { get; set; }

    }
}