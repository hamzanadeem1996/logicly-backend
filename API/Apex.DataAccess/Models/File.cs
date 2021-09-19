using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Files")]
    [PrimaryKey("Id")]
    public class File
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Type { get; set; }

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