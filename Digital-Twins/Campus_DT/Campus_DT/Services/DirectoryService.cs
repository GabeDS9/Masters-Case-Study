using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    class DirectoryService
    {
        Communication.ClientSocket myClient = new Communication.ClientSocket();
        public void RunDirectoryService()
        {
            while (true)
            {
                myClient.CommunicateWithGateway("DirectoryService");
            }
        }
    }
}
