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
using Resources;
using Models;

namespace Services
{
    public class ServiceGateway
    {
        DirectoryService directoryService = new DirectoryService();
        ExploratoryAnalyticsService exploratoryService = new ExploratoryAnalyticsService();

        Communication.ServerSocket myServer = new Communication.ServerSocket();
        Communication.ClientSocket myClient = new Communication.ClientSocket();

        private int ServerPort = 9000;

        public void InitialiseServices()
        {
            _ = directoryService.InitialiseDirectoryServiceAsync();
            StartGatewayServer();            
        }
        private void StartGatewayServer()
        {
            myServer.SetupServer(ServerPort, this);
        }
        public async Task<string> MessageHandlerAsync(string mes)
        {
            string message = "";
            var tempMessage = JsonConvert.DeserializeObject<UIMessageModel>(mes);

            if (tempMessage.ServiceTag == "Directory")
            {
                List<string> DTList = new List<string>();
                DTList = directoryService.ReturnDTs(tempMessage.DigitalTwin);
                message = JsonConvert.SerializeObject(DTList);
            }
            else if(tempMessage.ServiceTag == "Exploratory")
            {
                int port = directoryService.ReturnPortNumber(tempMessage.DigitalTwin);
                message = await exploratoryService.ExploratoryServiceAsync(port, tempMessage);
            }
            return message;
        }
    }
}
