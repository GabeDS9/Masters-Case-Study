using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Building_DT
{    public class Building
    {
        // Intialise building specific info
        public string Building_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public List<EnergyMeterData> EnergyMeters { get; set; }
        public List<OccupancyMeterData> OccupancyMeters { get; set; }
        public List<SolarMeterData> SolarMeters { get; set; }

        private EnergyMeters energyManager = new EnergyMeters();
        private OccupancyMeters occupancyManager = new OccupancyMeters();
        private SolarMeters solarManager = new SolarMeters();

        public Building(string name, string latitude, string longitude)
        {
            Building_name = name;
            Latitude = latitude;
            Longitude = longitude;
            InitialiseBuilding();
        }

        // Get available energy meters in the building
        public void InitialiseBuilding()
        {
            EnergyMeters = energyManager.LoadEnergyMeterList(Building_name);
            OccupancyMeters = occupancyManager.LoadOccupancyMeterList();
            SolarMeters = solarManager.LoadSolarMeterList();
        }
    }
}
