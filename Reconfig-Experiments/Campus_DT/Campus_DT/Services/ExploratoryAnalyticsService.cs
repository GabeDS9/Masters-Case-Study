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
        Communication.ClientSocket myClient = new Communication.ClientSocket();
        public async Task<string> ExploratoryServiceAsync(int port, UIMessageModel message)
        {
            var temp = new MessageModel {
                DataType = message.DataType, MessageType = message.InformationType,
                DisplayType = message.DisplayType, DTDetailLevel = message.DTDetailLevel, startDate = message.startDate, endDate = message.endDate,
                timePeriod = message.timePeriod
            };
            var tempDTMessage = JsonConvert.SerializeObject(temp);
            return await myClient.sendMessageAsync(tempDTMessage, port);
        }
    }
}
