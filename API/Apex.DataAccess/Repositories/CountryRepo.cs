using Apex.DataAccess.Models;
using NPoco;

namespace Apex.DataAccess.Repositories
{
    public class CountryRepo
    {
        public Page<Country> GetCountries(int pagenumber, int pagesize)
        {
            var cmd = Sql.Builder.Append("SELECT Countries.*, IF(id = 231, 1, 0) AS priority FROM Countries ORDER BY priority DESC, name ASC");
            using (var db = Utility.Database)
            {
                var result = db.Page<Country>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public Page<State> GetStates(int pagenumber, int pagesize, int countryid = 0)
        {
            var cmd = Sql.Builder.Select("*").From("States");
            if (countryid > 0)
            {
                cmd.Where("CountryId=@0", countryid);
            }
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<State>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public Page<City> GetCities(int pagenumber, int pagesize, int stateid = 0)
        {
            var cmd = Sql.Builder.Select("*").From("Cities");
            if (stateid > 0)
            {
                cmd.Where("StateId=@0", stateid);
            }
            cmd.OrderBy("Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<City>(pagenumber, pagesize, cmd);
                return result;
            }
        }
    }
}