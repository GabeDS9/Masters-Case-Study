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
    public class ServiceGateway : ServiceBaseClass
    {
        private static LoadExcel excel = new LoadExcel();
        Communication.ServerSocket myServer = new Communication.ServerSocket();
        Communication.ClientSocket myClient = new Communication.ClientSocket();
        private int servicePort = 0;
        private List<Service> servicesList = new List<Service>();
        public void InitialiseServiceGateway()
        {
            servicesList = excel.LoadServices();
            foreach (var service in servicesList)
            {
                if (service.ServiceName == "Service Gateway")
                {
                    servicePort = service.Port;
                    break;
                }
            }
            Console.WriteLine($"Setting up service gateway on {servicePort}");
            StartGatewayServer(servicePort);
        }
        private void StartGatewayServer(int port)
        {
            myServer.SetupServer(port, this, null, null);
        }
        public async Task<string> MessageHandlerAsync(string mes)
        {
            string message = "";
            var tempMessage = JsonConvert.DeserializeObject<UIMessageModel>(mes);

            if (tempMessage.ServiceTag == "Directory")
            {
                foreach(var service in servicesList)
                {
                    if (service.ServiceName == "Directory Service")
                    {
                        var DTList = await myClient.sendMessageAsync(mes, service.IP_Address, service.Port);
                        message = DTList;
                        break;
                    }
                }                
            }
            else if(tempMessage.ServiceTag == "Exploratory")
            {
                foreach (var service in servicesList)
                {
                    if (service.ServiceName == "Exploratory Service")
                    {
                        var response = await myClient.sendMessageAsync(mes, service.IP_Address, service.Port);
                        message = response;
                        break;
                    }
                }
            }
            return message;
        }
    }
}
