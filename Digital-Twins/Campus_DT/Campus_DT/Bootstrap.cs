using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Campus_DT
{
    class Bootstrap
    {
        static CampusManager myCampus = new CampusManager();
        static Services.ServicesManager myServices = new Services.ServicesManager();
        public static void Main(String[] args)
        {
            Thread DTsThread = new Thread(myCampus.InitialiseCampus)
            {
                Name = "DT Thread"
            };
            /*Thread ServicesThread = new Thread(myServices.InitialiseServices)
            {
                Name = "Services Thread"
            };

            ServicesThread.Start();*/
            DTsThread.Start();            
        }
    }
}
