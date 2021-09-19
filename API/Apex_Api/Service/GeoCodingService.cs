using Geocoding;
using Geocoding.Google;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apex_Api.Service
{
    public class GeoCodingService
    {
        public class Coordinates
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string CityName { get; set; }
        }

        public async Task<Coordinates> GeoCoordinate(string address)
        {
            var coordinates = new Coordinates();
            IGeocoder geocoder = new GoogleGeocoder() { ApiKey = Constant.GOOGLE_API_KEY };
            IEnumerable<Address> addresses = await geocoder.GeocodeAsync(address);
            coordinates.Latitude = addresses.First().Coordinates.Latitude;
            coordinates.Longitude = addresses.First().Coordinates.Longitude;
            coordinates.CityName = ((GoogleAddress)addresses.First()).Components[0].LongName;
            return coordinates;
        }
    }
}