using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("ClinicianLocation")]
    [PrimaryKey("Id")]
    public class ClinicianLocation
    {
        public int Id { get; set; }
        public int ClinicianId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [JsonIgnore] public string Country { get; set; }
        [JsonIgnore] public string State { get; set; }
        [JsonIgnore] public string City { get; set; }
        [JsonIgnore] public DateTime UpdatedOn { get; set; }
    }
}