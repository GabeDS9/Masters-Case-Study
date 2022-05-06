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
            //exploratoryService.InitialiseEAService();
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
            int port = directoryService.ReturnPortNumber(tempMessage.DigitalTwin);
            var temp = new MessageModel {
                DataType = tempMessage.DataType, MessageType = tempMessage.InformationType,
                DisplayType = tempMessage.DisplayType, LowestDTLevel = tempMessage.LowestDTLevel, startDate = tempMessage.startDate, endDate = tempMessage.endDate,
                timePeriod = tempMessage.timePeriod
            };
            var tempDTMessage = JsonConvert.SerializeObject(temp);
            message = await myClient.sendMessageAsync(tempDTMessage, port);
            /*if (tempMessage.DataType == "Energy")
            {
                if(tempMessage.InformationType == "CurrentData")
                {
                    if(tempMessage.DisplayType == "Individual")
                    {
                        var temp = new MessageModel {
                            DataType = tempMessage.DataType, MessageType = tempMessage.InformationType,
                            DisplayType = tempMessage.DisplayType, startDate = tempMessage.startDate, endDate = tempMessage.endDate,
                            timePeriod = tempMessage.timePeriod
                        };
                        var tempDTMessage = JsonConvert.SerializeObject(temp);
                        message = await myClient.sendMessageAsync(tempDTMessage, port);
                    }
                    else if(tempMessage.DisplayType == "Collective")
                    {
                        var temp = new MessageModel {
                            DataType = tempMessage.DataType, MessageType = tempMessage.InformationType,
                            DisplayType = tempMessage.DisplayType, startDate = tempMessage.startDate, endDate = tempMessage.endDate,
                            timePeriod = tempMessage.timePeriod
                        };
                        var tempDTMessage = JsonConvert.SerializeObject(temp);
                        message = await myClient.sendMessageAsync(tempDTMessage, port);
                    }                    
                }
                else if (tempMessage.InformationType == "Averages")
                {
                    if (tempMessage.DisplayType == "Individual")
                    {
                        var temp = new MessageModel {
                            DataType = tempMessage.DataType, MessageType = tempMessage.InformationType,
                            DisplayType = tempMessage.DisplayType, startDate = tempMessage.startDate, endDate = tempMessage.endDate,
                            timePeriod = tempMessage.timePeriod
                        };
                        var tempDTMessage = JsonConvert.SerializeObject(temp);
                        message = await myClient.sendMessageAsync(tempDTMessage, port);
                    }
                    else if (tempMessage.DisplayType == "Collective")
                    {
                        var temp = new MessageModel {
                            DataType = tempMessage.DataType, MessageType = tempMessage.InformationType,
                            DisplayType = tempMessage.DisplayType, startDate = tempMessage.startDate, endDate = tempMessage.endDate,
                            timePeriod = tempMessage.timePeriod
                        };
                        var tempDTMessage = JsonConvert.SerializeObject(temp);
                        message = await myClient.sendMessageAsync(tempDTMessage, port);
                    }
                }
            }*/

            return message;
        }
    }
}
