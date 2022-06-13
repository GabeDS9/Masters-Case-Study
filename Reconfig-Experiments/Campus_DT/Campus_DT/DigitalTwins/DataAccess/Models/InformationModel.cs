using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class InformationModel
    {
        public string DataType { get; set; }
        public string DT_Type { get; set; }
        public string DT_name { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public double Value { get; set; }
        public string Timestamp { get; set; }
    }
}
