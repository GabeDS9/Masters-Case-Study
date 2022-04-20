using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Precinct_DT
{
    public class Precinct
    {
        public string Precinct_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public List<Energy_Reticulation_DT.EnergyReticulation> EnergyMeters { get; set; }
        public List<Solar_Reticulation_DT.SolarReticulation> SolarMeters { get; set; }
        public List<Building_DT.Building> Buildings { get; set; }

        private Energy_Reticulation_DT.EnergyReticulationManager energyManager = new Energy_Reticulation_DT.EnergyReticulationManager();
        private Solar_Reticulation_DT.SolarReticulationManager solarManager = new Solar_Reticulation_DT.SolarReticulationManager();
        private Building_DT.BuildingManager buildingManager = new Building_DT.BuildingManager();

        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        public Precinct(string name, string latitude, string longitude)
        {
            Precinct_name = name;
            Latitude = latitude;
            Longitude = longitude;
            InitialisePrecinct();
        }

        public void InitialisePrecinct()
        {
            Buildings = buildingManager.InitialiseBuildings(Precinct_name);
        }
    }
}
