using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class ChildDTModel
    {
        public string Name { get; set; }
        public string DT_Type { get; set; }

        public ChildDTModel(string name, string dtType)
        {
            Name = name;
            DT_Type = dtType;
        }
    }
}
