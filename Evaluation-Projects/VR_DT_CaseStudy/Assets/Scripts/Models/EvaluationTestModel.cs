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
        public float TimeTaken { get; set; }
        public float RAMusage { get; set; }
        public float CPUusage { get; set; }
    }
}
