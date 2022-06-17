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
        public double Power_Tot { get; set; }
        public double Power_Diff { get; set; }
        public string TimestampDay { get; set; }
        public string TimestampHours { get; set; }

        public EnergyMeterModel(string energymeter_name, int meter_id, double power_tot, double power_diff, string timestampDay, string timestampHour)
        {
            EnergyMeter_name = energymeter_name;
            Meter_ID = meter_id;
            Power_Tot = power_tot;
            Power_Diff = power_diff;
            TimestampDay = timestampDay;
            TimestampHours = timestampHour;
        }
    }
}
