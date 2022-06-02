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
        public double RAMusageMB { get; set; }
        public double RAMusagePerc { get; set; }
        public double CPUusage { get; set; }
    }
}
