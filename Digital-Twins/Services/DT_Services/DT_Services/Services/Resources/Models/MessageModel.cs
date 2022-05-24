using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class MessageModel
    {
        public string DataType { get; set; }
        public string MessageType { get; set; }
        public string DisplayType { get; set; }
        public List<string> DTDetailLevel { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string timePeriod { get; set; }
    }
}
