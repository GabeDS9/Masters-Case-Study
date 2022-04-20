using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DT_Services
{
    class ServiceGateway
    {
        public void StartGateway()
        {
            SocketServer.AsynchronousSocketListener myServer = new SocketServer.AsynchronousSocketListener();
            myServer.StartServer();
        }
    }
}
