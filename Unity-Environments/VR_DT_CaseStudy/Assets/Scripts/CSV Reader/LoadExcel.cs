using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadExcel
{
    List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
    private string ConfigurationFile = "ServiceGatewayCommunicationConfiguration.csv";

    public ServiceGateway LoadServiceGatewayAddress()
    {
        ServiceGateway serviceGateway = new ServiceGateway();
        List<Dictionary<string, object>> data = CSVReader.Read(ConfigurationFile);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["IP_Address"].ToString() != "-") && (data[i]["Port"].ToString() != "-"))
            {
                string ipAdd = data[i]["IP_Address"].ToString();
                int port = int.Parse(data[i]["Port"].ToString());

                var tempGateway = new ServiceGateway { IP_Address = ipAdd, Port = port };
                return tempGateway;
            }
        }
        return null;
    }    
}
