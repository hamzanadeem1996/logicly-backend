using Apex.DataAccess;
using Apex.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using NPoco;
using System;

namespace Apex_Api.Service
{
    public class AgencyService
    {
        public Agency GetAgency(int id)
        {
            var cmd = Sql.Builder.Select("*").From("Agencies");
            cmd.Where("Id=@0", id);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<Agency>(cmd);
                return result;
            }
        }

        public Agency SaveAgency(Agency res, IUserService userService, IConfiguration Configuration, int currentLoggedUser)
        {
            var data = GetAgency(res.Id) ?? new Agency();
            data.Latitude = res.Latitude;
            data.Longitude = res.Longitude;
            data.Name = res.Name;
            data.Email = res.Email;
            data.PlanId = res.PlanId;
            data.MaxSessionHours = res.MaxSessionHours;
            data.StripeCustomerId = res.StripeCustomerId;
            data.IsActive = res.IsActive;
            data.Address = res.Address;

            if (res.Id == 0)
            {
                var mat = GoogleMapsService.GetLocationInfo(res.Latitude, res.Longitude);
                if (mat != null)
                {
                    data.Country = mat.Country;
                    data.State = mat.State;
                    data.City = mat.City;
                    data.ZipCode = mat.ZipCode;
                    //data.Address = mat.Address;
                }
            }

            using (var db = Utility.Database)
            {
                if (data.Id == 0)
                {
                    data.AddedOn = DateTime.UtcNow;
                    data.AddedBy = currentLoggedUser;
                }
                data.LastModOn = DateTime.UtcNow;
                data.AddedBy = currentLoggedUser;
                db.Save(data);

                // check email exist when save user/clinician
                var exist = Common.Instances.User.CheckEmail(data.Email);
                if (exist == null)
                {
                    var srv = new ServiceUser();
                    var user = new User
                    {
                        FirstName = data.Name,
                        LastName = string.Empty,
                        Email = data.Email,
                        Password = Utility.FakerInstance.Random.AlphaNumeric(8),
                        RoleId = 1,
                        Lat = data.Latitude,
                        Long = data.Longitude,
                        PhoneNumber = "",
                        AgencyId = data.Id,
                        CityName = data.City,
                        CountryCode = 0,
                        CityId = 0,
                        Address = data.Address
                    };
                    srv.save(user, userService, Configuration);
                }
                return data;
            }
        }
    }
}