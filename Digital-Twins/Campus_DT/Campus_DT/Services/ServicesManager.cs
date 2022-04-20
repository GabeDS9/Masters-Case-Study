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
        public void InitialiseServices()
        {
            myGateway.StartGateway();
        }
    }
}
