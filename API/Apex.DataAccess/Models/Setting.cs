using FluentValidation;
using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Settings")]
    [PrimaryKey("Id")]
    public class Setting
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int UserId { get; set; }

        public string Units { get; set; }
        public string RoutingApp { get; set; }
        public int TreatmentSessionLength { get; set; }
        public int EvaluationSessionLength { get; set; }
        public int AdmissionSessionLength { get; set; }
        public int DischargeSessionLength { get; set; }
        public int RecertSessionLength { get; set; }
        public int ThirtyDayReEvalSessionLength { get; set; }
        public string DistanceCalculator { get; set; }
        public string IncludeWeekendsInWeekView { get; set; }
        public string WorkingHours { get; set; }
        public string PatinetNameFormat { get; set; }
        public string ColorCoding { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        [JsonIgnore]
        public int AddedBy { get; set; }

        [JsonIgnore]
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }
    }

    public class SettingsValidator : AbstractValidator<Setting>
    {
        public SettingsValidator()
        {
            RuleFor(x => x.Units).Must(x => x.Equals("MILES") || x.Equals("Miles") || x.Equals("KILOMETERS") || x.Equals("Kilometers")).WithMessage("Please only use: MILES,Miles,KILOMETERS,Kilometers");
            RuleFor(x => x.RoutingApp).Must(x => x.Equals("WAZE") || x.Equals("GOOGLE MAPS") || x.Equals("I AM LOST") || x.Equals("FIND MY WAY") || x.Equals("JUST GET ME THERE") || x.Equals("Waze") || x.Equals("Google Maps") || x.Equals("I am Lost") || x.Equals("Find My Way") || x.Equals("Just Get Me There")).WithMessage("Please only use: WAZE,GOOGLE MAPS,I AM LOST,FIND MY WAY,JUST GET ME THERE,Waze,Google Maps,I am Lost,Find My Way,Just Get Me There");
            RuleFor(x => x.TreatmentSessionLength).NotEmpty().WithMessage("Please fill the value.");
            RuleFor(x => x.EvaluationSessionLength).NotEmpty().WithMessage("Please fill the value.");
            RuleFor(x => x.AdmissionSessionLength).NotEmpty().WithMessage("Please fill the value.");
            RuleFor(x => x.DistanceCalculator).NotEmpty().WithMessage("Please fill the value.");
            RuleFor(x => x.IncludeWeekendsInWeekView).NotEmpty().WithMessage("Please fill the value.");
            RuleFor(x => x.WorkingHours).NotEmpty().WithMessage("Please fill the value.");
            RuleFor(x => x.PatinetNameFormat).NotEmpty().WithMessage("Please fill the value.");
            RuleFor(x => x.ColorCoding).NotEmpty().WithMessage("Please fill the value.");
        }
    }
}