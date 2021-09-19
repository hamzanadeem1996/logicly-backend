using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Roles")]
    [PrimaryKey("Id")]
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddedBy { get; set; }
        public int LastModBy { get; set; }

        [JsonIgnore]
        public DateTime AddedOn { get; set; }

        [JsonIgnore]
        public DateTime LastModOn { get; set; }
    }
}