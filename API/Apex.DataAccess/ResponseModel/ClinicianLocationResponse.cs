using System;

namespace Apex.DataAccess.ResponseModel
{
    public class ClinicianLocationResponse
    {
        public int Id { get; set; }
        public int ClinicianId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string IconImage => Utility.LIVE_LOGO_URL;
        public DateTime UpdatedOn { get; set; }
    }
}