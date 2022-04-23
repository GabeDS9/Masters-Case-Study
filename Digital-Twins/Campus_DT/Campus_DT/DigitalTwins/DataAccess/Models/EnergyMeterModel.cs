using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccess.Models
{
    public class EnergyMeterModel
    {
        // Intialise building specific info
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string EnergyMeter_name { get; set; }
        public int Meter_ID { get; set; }
        public double? Power_Tot { get; set; }
        public string? Timestamp { get; set; }

        public EnergyMeterModel(string energymeter_name, int meter_id, double power_tot, string timestamp){
            EnergyMeter_name = energymeter_name;
            Meter_ID = meter_id;
            Power_Tot = power_tot;
            Timestamp = timestamp;
        }
    }
}
