using NPoco;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Countries")]
    [PrimaryKey("Id")]
    public class Country
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}