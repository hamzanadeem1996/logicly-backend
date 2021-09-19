using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Plans")]
    [PrimaryKey("Id")]
    public class Plan
    {
        public int Id { get; set; }
        public string StripeProductId { get; set; }
        public string Name { get; set; }
        [JsonIgnore] public DateTime AddedOn { get; set; }
        [JsonIgnore] public int AddedBy { get; set; }
        [JsonIgnore] public DateTime LastModOn { get; set; }
        [JsonIgnore] public int LastModBy { get; set; }
    }
}