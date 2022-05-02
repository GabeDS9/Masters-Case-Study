using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    class DirectoryService
    {
        private List<DigitalTwin> digitalTwinsList = new List<DigitalTwin>();
        private LoadExcel loadExcel = new LoadExcel();
        
        public void InitialiseDirectoryService()
        {
            digitalTwinsList = loadExcel.LoadDigitalTwins();
        }

    }
}
