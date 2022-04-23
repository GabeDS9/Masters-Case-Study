using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class PrecinctModel
    {
        public string Precinct_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        //public List<Energy_Reticulation_DT.EnergyReticulation> EnergyMeters { get; set; }
        //public List<Solar_Reticulation_DT.SolarReticulation> SolarMeters { get; set; }
        public List<Building_DT.Building> Buildings { get; set; }

        public PrecinctModel(string precinctName, string latitude, string longitude, List<Building_DT.Building> buildings)
        {
            Precinct_name = precinctName;
            Latitude = latitude;
            Longitude = longitude;
            Buildings = buildings;
        }
    }
}
