using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class EnergyMeterModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string EnergyMeter_name { get; set; }
        public int Meter_ID { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public double Power_Tot { get; set; }
        public string Timestamp { get; set; }

        public EnergyMeterModel(string energymeter_name, int meter_id, string latitude, string longitude, double power_tot, string timestamp)
        {
            EnergyMeter_name = energymeter_name;
            Meter_ID = meter_id;
            Latitude = latitude;
            Longitude = longitude;
            Power_Tot = power_tot;
            Timestamp = timestamp;
        }
    }
}
