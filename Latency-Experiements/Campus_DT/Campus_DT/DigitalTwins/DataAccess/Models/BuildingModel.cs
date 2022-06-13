using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccess.Models
{
    public class BuildingModel
    {
        // Intialise building specific info
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Building_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public List<EnergyMeterModel> EnergyMeters { get; set; }
        public List<OccupancyMeterData> OccupancyMeters { get; set; }
        public List<SolarMeterData> SolarMeters { get; set; }

        public BuildingModel(string building_name, string latitude, string longitude, List<EnergyMeterModel> energyMeters, 
            List<OccupancyMeterData> occupancyMeters, List<SolarMeterData> solarMeters)
        {
            Building_name = building_name;
            Latitude = latitude;
            Longitude = longitude;
            EnergyMeters = energyMeters;
            OccupancyMeters = occupancyMeters;
            SolarMeters = solarMeters;
        }

        
    }
}
