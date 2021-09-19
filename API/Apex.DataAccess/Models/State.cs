using NPoco;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("States")]
    [PrimaryKey("Id")]
    public class State
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }
        public int CountryId { get; set; }
    }
}