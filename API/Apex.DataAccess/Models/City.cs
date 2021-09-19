using NPoco;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Cities")]
    [PrimaryKey("Id")]
    public class City
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }
        public int StateId { get; set; }
    }
}