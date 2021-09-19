using NPoco;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    public class PatientVisitScheduleSave
    {
        [JsonIgnore]
        public int Id { get; set; }

        public List<Patient> Patients { get; set; }

        [JsonIgnore]
        public int NurseId { get; set; }

        public DateTime VisitDate { get; set; }
        public int AddedBy { get; set; }
    }

    public class Patient
    {
        public int ClinicianId { get; set; }
        public int PatientId { get; set; }
        public int RecertId { get; set; }
        public string colorType { get; set; }
        public DateTime RoutineVisitDate { get; set; }
        public DateTime CertEndDate { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsCombined { get; set; }

        [JsonIgnore]
        public int SortIndex { get; set; }

        [ResultColumn]
        public bool IsRecert => colorType == "R";
        
        public bool IsRecertWithinRange(DateTime visitDate)
        {
            
            return Utility.DateInRange(visitDate,
                CertEndDate.AddDays(-4), CertEndDate);
        }
    }

    [TableName("PatientVisitSchedule")]
    [PrimaryKey("Id")]
    public class PatientVisitSchedule
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int NurseId { get; set; }

        [JsonIgnore]
        public int RecertId { get; set; }

        [JsonIgnore]
        public int AddedBy { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public double NurseLat { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public double NurseLong { get; set; }

        public int PatientId { get; set; }

        public bool IsLocked { get; set; }

        [JsonIgnore] [ResultColumn] public bool IsCompleted { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public double PatientLat { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public double PatientLong { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string Title { get; set; }

        [JsonIgnore]
        public bool AllDay { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
        public DateTime RoutineVisitDate { get; set; }

        [JsonIgnore]
        public string colorType { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string colorChar
        {
            get
            {
                return colorType == "RV" ? "RV" : colorType.Substring(0, 1);
            }
        }

        [JsonIgnore] [ResultColumn] public string PatientName { get; set; }

        [JsonIgnore]
        public int SortIndex { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public string color { get { return Utility.GetColor(colorChar); } }

        [ResultColumn]
        [JsonIgnore]
        public string Units { get; set; }

        [JsonIgnore] [ResultColumn] public string PatientAddress { get; set; }

        [JsonIgnore]
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }
        [JsonIgnore] public string CombinationVisit { get; set; }

    }

    public class PatientVisitScheduleGet
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int NurseId { get; set; }

        public int PatientId { get; set; }
        public int RecertId { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public bool IsLocked { get; set; }

        [JsonIgnore] [ResultColumn] public bool IsCompleted { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string Title { get; set; }

        [JsonIgnore]
        public bool AllDay { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        [JsonIgnore]
        public string colorType { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string colorChar
        {
            get
            {
                return colorType == "RV" ? "RV" : colorType.Substring(0, 1);
                //return colorType.Substring(0, 1);
            }
        }

        [JsonIgnore]
        public int SortIndex { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public double PatientLat { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public double PatientLong { get; set; }

        public string Units { get; set; }
        
        [ResultColumn]
        public decimal DistanceValue => Units == "Kilometers"
            ? Math.Round(Convert.ToDecimal(Convert.ToDouble(Driven) / 1.609), 2)
            : Math.Round(Convert.ToDecimal(Driven), 2);

        public string Distance { get; set; }
        public string Duration { get; set; }

        public decimal Driven { get; set; }

        public int DurationMins { get; set; }

        [ResultColumn]
        [JsonIgnore]
        public string color
        {
            get { return Utility.GetColor(colorChar); }
        }

        [JsonIgnore] [ResultColumn] public string PatientAddress { get; set; }

        [JsonIgnore]
        public int AddedBy { get; set; }

        [JsonIgnore]
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }

        [JsonIgnore] public bool IsPrimary { get; set; }
        [JsonIgnore] public bool IsCombined { get; set; }
        [JsonIgnore] public string CombinationVisit { get; set; }
    }

    public class SinglePatientVisitSchedule
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int NurseId { get; set; }

        public int PatientId { get; set; }

        private string oT;
        public string OT { get { if (oT == Convert.ToString(NurseId)) { return $", OT"; } return $""; } set { this.oT = value; } }

        private string oTA;
        public string OTA { get { if (oTA == Convert.ToString(NurseId)) { return $", OTA"; } return $""; } set { this.oTA = value; } }

        private string pT;
        public string PT { get { if (pT == Convert.ToString(NurseId)) { return $", PT"; } return $""; } set { this.pT = value; } }

        private string pTA;
        public string PTA { get { if (pTA == Convert.ToString(NurseId)) { return $", PTA"; } return $""; } set { this.pTA = value; } }

        private string sLP;
        public string SLP { get { if (sLP == Convert.ToString(NurseId)) { return $", SLP"; } return $""; } set { this.sLP = value; } }

        private string sN;
        public string SN { get { if (sN == Convert.ToString(NurseId)) { return $", SN"; } return $""; } set { this.sN = value; } }

        private string aID;
        public string AID { get { if (aID == Convert.ToString(NurseId)) { return $", AID"; } return $""; } set { this.aID = value; } }

        private string msW;
        public string MSW { get { if (msW == Convert.ToString(NurseId)) { return $", MSW"; } return $""; } set { this.msW = value; } }

        private string title;

        [JsonIgnore]
        [ResultColumn]
        public string Title
        {
            get { return $"{title}{OT}{OTA}{PT}{PTA}{SLP}{SN}{AID}{MSW}"; }
            set { this.title = value; }
        }

        [JsonIgnore]
        public bool AllDay { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        [JsonIgnore]
        public string colorType { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public string colorChar
        {
            get
            {
                return colorType == "RV" ? "RV" : colorType.Substring(0, 1);
            }
        }

        [ResultColumn]
        [JsonIgnore]
        public string color { get { return Utility.GetColor(colorChar); } }

        [JsonIgnore]
        public int AddedBy { get; set; }

        [JsonIgnore]
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }
    }
}