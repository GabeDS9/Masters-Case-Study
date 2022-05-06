using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class DigitalTwin
    {
        public string DT_Name { get; set; }
        public int Port { get; set; }
        public List<string> Child_DTs { get; set; }
    }
}
