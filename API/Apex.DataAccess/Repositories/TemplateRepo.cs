using Apex.DataAccess.Models;
using NPoco;
using System.Collections.Generic;

namespace Apex.DataAccess.Repositories
{
    public class TemplateRepo
    {
        public List<Template> GetTemplate()
        {
            var cmd = Sql.Builder.Select("*").From("Templates");
            using (var db = Utility.Database)
            {
                var result = db.Fetch<Template>(cmd);
                return result;
            }
        }
    }
}