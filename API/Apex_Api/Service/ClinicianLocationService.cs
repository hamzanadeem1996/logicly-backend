using Apex.DataAccess;
using Apex.DataAccess.Models;
using Apex.DataAccess.ResponseModel;
using NPoco;
using System;

namespace Apex_Api.Service
{
    public class ClinicianLocationService
    {
        public ClinicianLocation SaveLocation(ClinicianLocation req)
        {
            var data = GetClinicianLocation(req.ClinicianId) ?? new ClinicianLocation();
            data.Latitude = req.Latitude;
            data.Longitude = req.Longitude;
            data.ClinicianId = req.ClinicianId;
            data.UpdatedOn = DateTime.UtcNow;

            var mat = GoogleMapsService.GetLocationInfo(req.Latitude, req.Longitude);
            if (mat != null)
            {
                data.Country = mat.Country;
                data.State = mat.State;
                data.City = mat.City;
            }
            data = Save(data);
            return data;
        }

        public object GetClinicianLocations(double latitude, double longitude, int pageNumber, int pageSize, int agencyId)
        {
            var res = new object();
            var mat = GoogleMapsService.GetLocationInfo(latitude, longitude);
            if (mat.State != null)
            {
                res = GetAllClinicianLocation(pageNumber, pageSize, mat.State, agencyId);
            }
            return res;
        }

        public Page<ClinicianLocationResponse> GetAllClinicianLocation(int pageNumber, int pageSize, string state, int agencyId)
        {
            var cmd = Sql.Builder.Select("c.*,u.FirstName,u.LastName").From("ClinicianLocation c");
            cmd.LeftJoin("Users u").On("u.Id=c.ClinicianId");
            cmd.Where("u.AgencyId=@0", agencyId);
            cmd.Where("c.UpdatedOn > UTC_TIME() - Interval 5 MINUTE");
            using (var db = Utility.Database)
            {
                var result = db.Page<ClinicianLocationResponse>(pageNumber, pageSize, cmd);
                return result;
            }
        }

        public ClinicianLocation GetClinicianLocation(int clinicianId)
        {
            var cmd = Sql.Builder.Select("*").From("ClinicianLocation");
            cmd.Where("ClinicianId=@0", clinicianId);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<ClinicianLocation>(cmd);
                return result;
            }
        }

        public ClinicianLocation Save(ClinicianLocation res)
        {
            using (var db = Utility.Database)
            {
                db.Save(res);
                return res;
            }
        }
    }
}