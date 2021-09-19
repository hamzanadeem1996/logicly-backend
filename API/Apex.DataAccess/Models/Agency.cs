using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Agencies")]
    [PrimaryKey("Id")]
    public class Agency
    {
        public int Id { get; set; }
        [JsonIgnore] public string StripePaymentMethodId { get; set; }
        [JsonIgnore] public string StripeCustomerId { get; set; }
        [ResultColumn] [JsonIgnore] public string StripePriceId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PlanId { get; set; }
        [ResultColumn] [JsonIgnore] public string PlanName { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [JsonIgnore] public string Country { get; set; }
        [JsonIgnore] public string State { get; set; }
        [JsonIgnore] public string City { get; set; }
        [JsonIgnore] public string ZipCode { get; set; }
        public string Address { get; set; }
        public string MaxSessionHours { get; set; }
        public bool IsActive { get; set; }
        [JsonIgnore] public int AddedBy { get; set; }
        [JsonIgnore] public DateTime AddedOn { get; set; }
        [JsonIgnore] public int LastModBy { get; set; }
        [JsonIgnore] public DateTime LastModOn { get; set; }
    }
}