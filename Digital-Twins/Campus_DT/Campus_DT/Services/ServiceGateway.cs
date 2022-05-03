using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Models;

namespace Services
{
    public class ServiceGateway
    {
        DirectoryService directoryService = new DirectoryService();
        ExploratoryAnalyticsService exploratoryService = new ExploratoryAnalyticsService();

        Communication.ServerSocket myServer = new Communication.ServerSocket();

        private int ServerPort = 9000;

        public void InitialiseServices()
        {            
            directoryService.InitialiseDirectoryService();
            StartGatewayServer();
            //exploratoryService.InitialiseEAService();
        }

        private void StartGatewayServer()
        {
            myServer.SetupServer(ServerPort);
        }

        /*public void RunGatewayService()
        {
            while (true)
            {
                String message = "";
                Socket clientSocket;
                (message, clientSocket) = myServer.ListenForMessages();
                //ProcessMessage(message, clientSocket);
            }
        }
        public void ProcessMessage(String message, Socket clientSocket)
        {
            String information = "Hello";

            myServer.SendMessage(information, clientSocket);
        }*/
    }
}
