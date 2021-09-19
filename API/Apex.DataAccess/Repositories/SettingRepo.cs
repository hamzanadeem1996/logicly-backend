using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class SettingRepo
    {
        public Setting Get(int userid)
        {
            var cmd = Sql.Builder.Select("*").From("Settings");
            cmd.Where("UserId=@0", userid);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<Setting>(cmd) ?? new Setting();
                return result;
            }
        }

        public object Save(Setting data, int userid = 0)
        {
            using (var db = Utility.Database)
            {
                if (data.Id == 0)
                {
                    data.AddedOn = DateTime.UtcNow;
                    data.AddedBy = userid;
                }
                data.LastModOn = DateTime.UtcNow;
                data.LastModBy = userid;
                data.UserId = userid;
                db.Save(data);
                return data;
            }
        }
    }
}