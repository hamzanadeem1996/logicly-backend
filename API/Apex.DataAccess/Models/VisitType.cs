using NPoco;
using System.Collections.Generic;

namespace Apex.DataAccess.Models
{
    [TableName("VisitTypes")]
    [PrimaryKey("Id")]
    public class VisitType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string color { get; set; }

        public List<VisitType> GetAll()
        {
            var cmd = Sql.Builder.Select("*").From("VisitTypes");

            using (var db = Utility.Database)
            {
                var result = db.Fetch<VisitType>(cmd);
                return result;
            }
        }
    }
}