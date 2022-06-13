using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Energy_Reticulation_DT
{
    public class EnergyReticulation
    {
        public string Reticulation_name { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public List<EnergyMeterData> EnergyMeters { get; set; }

        private EnergyMeters energyManager = new EnergyMeters();
        private APICaller apiCaller = new APICaller();
        private Stopwatch stopWatch = new Stopwatch();
    }
}
