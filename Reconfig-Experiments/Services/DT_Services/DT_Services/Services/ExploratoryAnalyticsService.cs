using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using Resources;

namespace Services
{
    public class ExploratoryAnalyticsService
    {
        private List<Service> servicesList = new List<Service>();
        private LoadExcel excel = new LoadExcel();
        Communication.ClientSocket myClient = new Communication.ClientSocket();
        Communication.ServerSocket myServer = new Communication.ServerSocket();
        int servicePort = 0;
        public void InitialiseExploratoryAnalyticsService()
        {
            servicesList = excel.LoadServices();
            foreach (var service in servicesList)
            {
                if (service.ServiceName == "Exploratory Service")
                {
                    servicePort = service.Port;
                    break;
                }
            }
            Console.WriteLine($"Setting up exploratory service on {servicePort}");
            StartServiceServer(servicePort);
        }
        private void StartServiceServer(int port)
        {
            Console.WriteLine($"Exploratory analytics service running");
            myServer.SetupServer(port, null, null, this, null);
        }
        public async Task<string> MessageHandlerAsync(string mes)
        {
            string message = "";
            var tempMessage = JsonConvert.DeserializeObject<UIMessageModel>(mes);
            foreach(var item in servicesList)
            {
                if(item.ServiceName == "Directory Service")
                {
                    string ipAdd = "";
                    int port = 0;
                    UIMessageModel tempMes = new UIMessageModel { DataType = "IP_Address", DigitalTwin = tempMessage.DigitalTwin };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    ipAdd = await myClient.sendMessageAsync(temp, item.IP_Address, item.Port);

                    tempMes = new UIMessageModel { DataType = "Port", DigitalTwin = tempMessage.DigitalTwin };
                    temp = JsonConvert.SerializeObject(tempMes);
                    port = int.Parse(await myClient.sendMessageAsync(temp, item.IP_Address, item.Port));

                    message = await ExploratoryServiceAsync(ipAdd, port, tempMessage);
                }
            }
            return message;
        }
        public async Task<string> ExploratoryServiceAsync(string ipAdd,int port, UIMessageModel message)
        {
            var temp = new MessageModel {
                DataType = message.DataType, MessageType = message.InformationType,
                DisplayType = message.DisplayType, DTDetailLevel = message.DTDetailLevel, startDate = message.startDate, endDate = message.endDate,
                timePeriod = message.timePeriod
            };
            var tempDTMessage = JsonConvert.SerializeObject(temp);
            return await myClient.sendMessageAsync(tempDTMessage, ipAdd, port);
        }
    }
}
