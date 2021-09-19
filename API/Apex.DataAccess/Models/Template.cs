using NPoco;

namespace Apex.DataAccess.Models
{
    [TableName("Templates")]
    [PrimaryKey("Id")]
    public class Template
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string Content { get; set; }
    }
}