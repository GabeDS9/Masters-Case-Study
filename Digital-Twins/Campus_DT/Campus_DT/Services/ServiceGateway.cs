using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    class ServiceGateway
    {
        Communication.ServerSocket myServer = new Communication.ServerSocket();

        public void StartGateway()
        {
            myServer.StartServer();
            RunGatewayService();
        }

        public void RunGatewayService()
        {
            String message = "";
            Socket clientSocket;
            (message, clientSocket) = myServer.ListenForMessages();
            ProcessMessage(message, clientSocket);
        }
        public void ProcessMessage(String message, Socket clientSocket)
        {
            String information = "Hello";

            myServer.SendMessage(information, clientSocket);
        }
    }
}
