﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MessageHandler
{
    private ClientSocket myClient = new ClientSocket();
    public async Task<List<string>> GetDTListAsync(string dt)
    {
        var message = new MessageModel
        {
            ServiceTag = "Directory",
            DataType = "List",
            InformationType = "",
            DisplayType = "",
            DigitalTwin = dt,
            DTDetailLevel = null,
            startDate = "",
            endDate = "",
            timePeriod = ""
        };
        var mes = JsonConvert.SerializeObject(message);
        var temp = await myClient.sendMessageAsync(mes, 9000);
        List<string> response = JsonConvert.DeserializeObject<List<string>>(temp);
        return response;
    }
}