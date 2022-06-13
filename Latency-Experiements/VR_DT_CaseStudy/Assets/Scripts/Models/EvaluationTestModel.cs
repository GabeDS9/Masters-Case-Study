using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class EvaluationTestModel
    {
        public int NumberOfDTs { get; set; }
        public int NumberOfCampuses { get; set; }
        public int NumberOfPrecincts { get; set; }
        public int NumberOfBuildings { get; set; }
        public int NumberOfDataPoints { get; set; }
        public float TotalTimeTaken { get; set; }
        public float DTResponseTimeTaken { get; set; }
        public float VisualTimeTaken { get; set; }
        public double RAMusageMBUnity { get; set; }
        public double RAMusageMBDTs { get; set; }
        public double RAMusageMBServices { get; set; }
        public double RAMusageMBTotal { get; set; }
        public double RAMusagePercUnity { get; set; }
        public double RAMusagePercDTs { get; set; }
        public double RAMusagePercServices { get; set; }
        public double RAMusagePercTotal { get; set; }
        public double CPUusageUnity { get; set; }
        public double CPUusageDTs { get; set; }
        public double CPUusageServices { get; set; }
        public double CPUusageTotal { get; set; }
    }
}
