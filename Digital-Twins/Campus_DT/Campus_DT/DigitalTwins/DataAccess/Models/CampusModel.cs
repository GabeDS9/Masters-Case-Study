using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class CampusModel
    {
        public static string Campus_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public static List<string> Precincts { get; set; }

        public CampusModel(string campus_name, string latitude, string longitude, List<string> precincts)
        {
            Campus_name = campus_name;
            Latitude = latitude;
            Longitude = longitude;
            Precincts = precincts;
        }
    }
}
