using Geocoding.Google;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using System.Collections.Generic;
using System.Linq;

namespace Apex_Api.Service
{
    public class GoogleMapsService
    {
        public class DistanceData
        {
            public decimal Distance { get; set; }
            public string DistanceStr { get; set; }
            public int DurationSeconds { get; set; }
            public int DurationMins => DurationSeconds / 60;
            public string DurationStr { get; set; }
            public decimal Duration { get; set; }
            public string DistanceFormatted => $"{Distance} miles";
            public string DurationFormatted { get; set; }
        }

        public DistanceData GetDistanceMatrix(double startLat, double startLon, double endLat, double endLon)
        {
            //var key = $"GetDistanceMatrix-{startLat}-{startLon}-{endLat}-{endLon}";
            //if (Cache.Exists(key)) return (DistanceData)Cache.Get(key);
            var req = new DistanceMatrixRequest();
            var distanceData = new DistanceData();
            req.Key = Constant.GOOGLE_API_KEY;
            req.Units = Units.Imperial;
            var origins = new List<Location> { new Location { Latitude = startLat, Longitude = startLon } };
            req.Origins = origins;

            var dest = new List<Location> { new Location { Latitude = endLat, Longitude = endLon } };
            req.Destinations = dest;
            var res = GoogleApi.GoogleMaps.DistanceMatrix.Query(req);
            if (res?.Rows != null)
            {
                var data = res.Rows.FirstOrDefault()?.Elements.FirstOrDefault();
                if (data != null)
                {
                    distanceData.Distance = data.Distance?.Value ?? 0;
                    distanceData.DistanceStr = data.Distance?.Text ?? "";
                    distanceData.DurationSeconds = data.Duration?.Value ?? 0;
                    distanceData.DurationStr = data.Duration?.Text ?? "";
                    //convert to miles
                    distanceData.Distance = System.Convert.ToDecimal(UnitsNet.Length.FromMeters(distanceData.Distance).Miles);

                    //CONVERT TO HOURS IF MORE THAN 60 MINS
                    if (distanceData.DurationMins > 60)
                    {
                        distanceData.Duration = System.Convert
                            .ToDecimal(UnitsNet.Duration.FromSeconds(distanceData.DurationSeconds).Hours);
                        distanceData.DurationFormatted = $"{distanceData.Duration} hrs";
                    }
                    else
                    {
                        distanceData.DurationFormatted = $"{distanceData.Duration} mins";
                    }
                }
            }
            //Cache.Set(key, distanceData);
            return distanceData;
            ;
        }

        public class LocationInfo
        {
            public string Country { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
            public string Address { get; set; }
        }

        public static LocationInfo GetLocationInfo(double lat, double lon)
        {
            //var key = $"GetLocationInfo-{lat}-{lon}";
            //if (Cache.Exists(key)) return (LocationInfo)Cache.Get(key);

            var geoCoder = new GoogleGeocoder() { ApiKey = Constant.GOOGLE_API_KEY };
            var addresses = geoCoder.ReverseGeocodeAsync(lat, lon);
            var loc = new LocationInfo();
            foreach (var address in addresses.Result)
            {
                foreach (var cmp in address.Components)
                {
                    foreach (var type in cmp.Types)
                    {
                        if (type == GoogleAddressType.Country)
                        {
                            loc.Country = cmp.LongName ?? "";
                        }
                        else if (type == GoogleAddressType.AdministrativeAreaLevel1)
                        {
                            loc.State = cmp.LongName ?? "";
                        }
                        else if (type == GoogleAddressType.Locality)
                        {
                            loc.City = cmp.LongName ?? "";
                        }
                        else if (type == GoogleAddressType.PostalCode)
                        {
                            loc.ZipCode = cmp.LongName ?? "";
                        }
                        else if (type == GoogleAddressType.SubLocality)
                        {
                            loc.Address = cmp.LongName ?? "";
                        }
                    }
                }
            }
            return loc;
        }
    }
}