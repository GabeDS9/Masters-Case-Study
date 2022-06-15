using Models;
using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class EnergyCostService
    {
        private List<Service> servicesList = new List<Service>();
        private LoadExcel excel = new LoadExcel();
        Communication.ClientSocket myClient = new Communication.ClientSocket();
        Communication.ServerSocket myServer = new Communication.ServerSocket();
        int servicePort = 0;
        double energyCost = 1.209;
        public void InitialiseEnergyCostService()
        {
            servicesList = excel.LoadServices();
            foreach (var service in servicesList)
            {
                if (service.ServiceName == "Energy Cost Service")
                {
                    servicePort = service.Port;
                    break;
                }
            }
            Console.WriteLine($"Setting up energy cost service on {servicePort}");
            StartServiceServer(servicePort);
        }
        private void StartServiceServer(int port)
        {
            Console.WriteLine($"Energy Cost service running");
            myServer.SetupServer(port, null, null, null, this);
        }
        public async Task<string> MessageHandlerAsync(string mes)
        {
            string message = "";
            var tempMessage = JsonConvert.DeserializeObject<UIMessageModel>(mes);
            foreach (var item in servicesList)
            {
                if (item.ServiceName == "Directory Service")
                {
                    string ipAdd = "";
                    int port = 0;
                    UIMessageModel tempMes = new UIMessageModel { DataType = "IP_Address", DigitalTwin = tempMessage.DigitalTwin };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    ipAdd = await myClient.sendMessageAsync(temp, item.IP_Address, item.Port);

                    tempMes = new UIMessageModel { DataType = "Port", DigitalTwin = tempMessage.DigitalTwin };
                    temp = JsonConvert.SerializeObject(tempMes);
                    port = int.Parse(await myClient.sendMessageAsync(temp, item.IP_Address, item.Port));

                    message = await CostServiceAsync(ipAdd, port, tempMessage);
                }
            }
            return message;
        }
        public async Task<string> CostServiceAsync(string ipAdd, int port, UIMessageModel message)
        {
            var temp = new MessageModel
            {
                DataType = message.DataType,
                MessageType = "Total",
                DisplayType = message.DisplayType,
                DTDetailLevel = message.DTDetailLevel,
                startDate = message.startDate,
                endDate = message.endDate,
                timePeriod = message.timePeriod
            };
            var tempDTMessage = JsonConvert.SerializeObject(temp);
            var response = await myClient.sendMessageAsync(tempDTMessage, ipAdd, port);
            var decode = JsonConvert.DeserializeObject<List<InformationModel>>(response);
            foreach(var item in decode)
            {
                item.Value = item.Value * energyCost;
            }
            return JsonConvert.SerializeObject(decode);
        }
    }
}
