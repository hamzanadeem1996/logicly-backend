using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("PatientDates")]
    [PrimaryKey("Id")]
    public class PatientDate
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ClinicianId { get; set; }

        [JsonIgnore]
        public int ScheduleId { get; set; }

        [JsonIgnore]
        public int RecertId { get; set; }

        //[ResultColumn] public DateTime RecertEnd { get; set; }
        private string patientfirstname;

        [ResultColumn]
        public string PatientFirstName
        {
            get { return $"{Encryption.Decrypt(patientfirstname)}"; }
            set { this.patientfirstname = value; }
        }

        private string patientlastname;

        [ResultColumn]
        public string PatientLastName
        {
            get { return $"{Encryption.Decrypt(patientlastname)}"; }
            set { this.patientlastname = value; }
        }

        [ResultColumn] public string PatientName => $"{PatientFirstName} {PatientLastName}";
        [ResultColumn] public string PatientFullName { get; set; }

        [ResultColumn] public string NurseName { get; set; }

        public DateTime PatientDates { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string DischargeValue
           => PatientDates.Year == 0001 ? "N/A" : PatientDates.Year < 1970 ? "N/A" : PatientDates.ToString("O");

        [JsonIgnore]
        [ResultColumn]
        public string DischargeWeek
            => PatientDates.Year == 0001 ? "N/A" : DischargeValue != "N/A" ? Utility.GetDateRangeOfWeek(PatientDates) : "N/A";

        public string Type { get; set; }
        public bool IsAddedToSchedule { get; set; }

        public string Status { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public DateTime CertStartDate { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public DateTime CertEndDate
           => CertStartDate.AddDays(59);
        [JsonIgnore] public string FrequencyType { get; set; }
    }
}