using NPoco;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("PatientProfiles")]
    [PrimaryKey("Id")]
    public class PatientProfile
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredName { get; set; }
        public string Address { get; set; }
        public string PrimaryNumber { get; set; }
        public string SecondaryNumber { get; set; }
        public DateTime Admission { get; set; }
        [JsonIgnore] [ResultColumn] public string CurrentCertPeriod { get; set; }
        [JsonIgnore] [ResultColumn] public DateTime Evaluation { get; set; }
        [JsonIgnore] [ResultColumn] public bool EvalCompleted { get; set; }
        [JsonIgnore] [ResultColumn] public DateTime EndDate { get; set; }
        [JsonIgnore] [ResultColumn] public string Frequency { get; set; }

        //[JsonIgnore] [ResultColumn] public string MultipleFrequency { get; set; }

        [JsonIgnore] [ResultColumn] public int ActiveCertId { get; set; }

        [JsonIgnore] [ResultColumn] public DateTime Discharge { get; set; }
        [JsonIgnore] [ResultColumn] public DateTime Eoc { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string DischargeValue
            => Discharge.Year == 0001 ? "N/A" : Discharge.Year < 1970 ? "N/A" : Discharge.ToString("O");

        [JsonIgnore]
        [ResultColumn]
        public string DischargeWeek
            => Discharge.Year == 0001 ? "N/A" : DischargeValue != "N/A" ? Utility.GetDateRangeOfWeek(Discharge) : "N/A";

        [JsonIgnore] [ResultColumn] public DateTime ThirtyDaysRelEval { get; set; }

        [JsonIgnore] [ResultColumn] public string Most30DRE { get; set; }

        [JsonIgnore] [ResultColumn] public string MostRecent30DRE 
            => Most30DRE == "N/A" ? "N/A" : Convert.ToDateTime(Most30DRE).ToString("MMM dd, yyyy");


        [JsonIgnore]
        [ResultColumn]
        public string Upcoming30DRE 
            => MostRecent30DRE == "N/A" && ThirtyDaysRelEval.Year > 001 ? ThirtyDaysRelEval.ToString("MMM dd, yyyy") :
            MostRecent30DRE != "N/A" ? Convert.ToDateTime(MostRecent30DRE).AddDays(29).ToString("MMM dd, yyyy") : "N/A";
       

        [JsonIgnore]
        [ResultColumn]
        public string Recert =>
            Eoc.Year == 1 ? "N/A" : $"{Eoc.AddDays(-4).ToString("MMM dd, yyyy")} - {Eoc.ToString("MMM dd, yyyy")}";

        public string Notes { get; set; }
        public int UserId { get; set; }
        public string MDNumber { get; set; }
        public string MDName { get; set; }
        public int CareTeamId { get; set; }
        public int TeamLeader { get; set; }
        private string teamleadername { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string TeamLeaderName
        {
            get { return teamleadername == null ? "None" : teamleadername; }
            set { this.teamleadername = value; }
        }

        public int OT { get; set; }
        public int OTA { get; set; }
        public int PT { get; set; }
        public int PTA { get; set; }
        public int SLP { get; set; }
        public int SN { get; set; }
        public int AID { get; set; }
        public int MSW { get; set; }
        private string otname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string OTNAME
        {
            get { return otname == null ? "None" : otname; }
            set { this.otname = value; }
        }

        private string otaname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string OTAName
        {
            get { return otaname == null ? "None" : otaname; }
            set { this.otaname = value; }
        }

        private string ptname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string PTName
        {
            get => ptname ?? "None";
            set => ptname = value;
        }

        private string ptaname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string PTAName
        {
            get { return ptaname == null ? "None" : ptaname; }
            set { this.ptaname = value; }
        }

        private string slpname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string SLPName
        {
            get { return slpname == null ? "None" : slpname; }
            set { this.slpname = value; }
        }

        private string snname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string SNName
        {
            get { return snname == null ? "None" : snname; }
            set { this.snname = value; }
        }

        private string aidname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string AIDName
        {
            get { return aidname == null ? "None" : aidname; }
            set { this.aidname = value; }
        }

        private string mswname { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string MSWName
        {
            get { return mswname == null ? "None" : mswname; }
            set { this.mswname = value; }
        }

        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }

        //[ResultColumn]
        //public string FullName
        //{
        //    get
        //    {
        //        return string.IsNullOrEmpty(PreferredName) ? $"{FirstName} {LastName}" : $"{PreferredName} {LastName}";
        //    }
        //}


        [ResultColumn]
        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(LastName) ? $"{FirstName} {PreferredName}" : $"{FirstName} {LastName}";
            }
        }



        [JsonIgnore]
        public string Status { get; set; }

        [JsonIgnore]
        public int AddedBy { get; set; }

        [JsonIgnore]
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public int CertificationPeriodCount { get; set; }
    }

    public class Schedule
    {
        public bool AllDay { get; set; }
        public int PatientId { get; set; }
        public int RecertId { get; set; }
        public string CityName { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public int NoOfVisits { get; set; }
        public int VisitsPerWeek { get; set; }
        public string color { get; set; }
        public bool IsDisabled { get; set; }
        public string colorType { get; set; }
        public string Distance { get; set; }

        [ResultColumn]
        public List<VType> vsttype { get; set; }
    }

    public class VType
    {
        public int Id { get; set; }
        public int RecertId { get; set; }
        public DateTime CertStartDate { get; set; }
        public DateTime CertEndDate { get; set; }
        public int ClinicianId { get; set; }
        public string Visitcolor { get; set; }
        public string VisitTypeCode { get; set; }
        public DateTime RoutineVisitDate { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsCombined { get; set; }
        public int Sortby { get; set; }
    }

    public class PatientCsvResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string InitialAdmission { get; set; }
        public string Address { get; set; }
        public string MDNumber { get; set; }
        public string PrimaryNumber { get; set; }
        public string SecondaryNumber { get; set; }
        public string Status { get; set; }
    }
}