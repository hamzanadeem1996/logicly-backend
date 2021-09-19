using Apex.DataAccess.Models;
using System;

namespace Apex.DataAccess.Repositories
{
    public class FileRepo
    {
        public File Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<File>(id) ?? new File();
                return result;
            }
        }

        public object Save(File files)
        {
            using (var db = Utility.Database)
            {
                if (files.Id == 0)
                {
                    files.AddedOn = DateTime.UtcNow;
                }
                files.LastModOn = DateTime.UtcNow;
                db.Save(files);
                return files;
            }
        }
    }
}