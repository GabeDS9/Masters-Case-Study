using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    class ServicesManager
    {
        ServiceGateway myGateway = new ServiceGateway();
        DirectoryService directoryService = new DirectoryService();
        ExploratoryAnalyticsService exploratoryService = new ExploratoryAnalyticsService();
        public void InitialiseServices()
        {
            myGateway.StartGatewayAsync();
            directoryService.InitialiseDirectoryService();
            exploratoryService.InitialiseEAService();
        }
    }
}
