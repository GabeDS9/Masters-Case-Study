using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Building_DT
{
    class EnergyMeterData
    {
        public string meterid { get; set; }
        public string description { get; set; }
        public string make { get; set; }
        public string type { get; set; }
        public string serial_no { get; set; }
        public string yard_no { get; set; }
        public string building_no { get; set; }
        public string floor { get; set; }
        public string room_no { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public List<EnergyData> data { get; set; }
    }
}
