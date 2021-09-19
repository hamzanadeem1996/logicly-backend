using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Recertifications")]
    [PrimaryKey("Id")]
    public class Recertification
    {
        [JsonIgnore] public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime RecertificationDate { get; set; }
        [JsonIgnore] [ResultColumn] public DateTime Evaluation { get; set; }
        [JsonIgnore] public int AddedBy { get; set; }
        [JsonIgnore] public DateTime AddedOn { get; set; }
        [JsonIgnore] public int LastModBy { get; set; }
        [JsonIgnore] public DateTime LastModOn { get; set; }
    }
}