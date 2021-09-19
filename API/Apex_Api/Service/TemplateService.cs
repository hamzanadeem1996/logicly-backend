using Apex.DataAccess;
using Apex.DataAccess.Models;
using NPoco;

namespace Apex_Api.Service
{
    public class TemplateService
    {
        public Template GetByKey(string templateKey)
        {
            var cmd = Sql.Builder.Select("*").From("Templates");
            cmd.Where("TemplateName=@0", templateKey);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<Template>(cmd);
                return result;
            }
        }
    }
}