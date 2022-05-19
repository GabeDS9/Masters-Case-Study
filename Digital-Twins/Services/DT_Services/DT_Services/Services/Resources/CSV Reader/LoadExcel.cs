using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace Services
{
    public class LoadExcel
    {
        public List<DigitalTwin> digitalTwinsList = new List<DigitalTwin>();

        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

        private string DTConfigurationFile = "LargeDTArchitectureConfiguration.csv";

        #region DigitalTwins
        public List<DigitalTwin> LoadDigitalTwins()
        {
            digitalTwinsList.Clear();

            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

            List<Dictionary<string, object>> data = CSVReader.Read(filepath);

            for (var i = 0; i < data.Count; i++)
            {
                // Add Campus DT
                if((data[i]["Precinct"].ToString() == "-") && (data[i]["Port"].ToString() != "0"))
                {
                    string dt_name = data[i]["Campus"].ToString();
                    string ip = data[i]["IP_Address"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, IP_Address = ip, Port = port });
                }
                else if((data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() == "-") && (data[i]["Port"].ToString() != "0"))
                {
                    string dt_name = data[i]["Precinct"].ToString();
                    string ip = data[i]["IP_Address"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, IP_Address = ip, Port = port });
                }
                else if ((data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-") && (data[i]["Port"].ToString() != "0"))
                {
                    string dt_name = data[i]["Reticulation"].ToString();
                    string ip = data[i]["IP_Address"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, IP_Address = ip, Port = port });
                }
                else if ((data[i]["Precinct"].ToString() != "-") && (data[i]["Building"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-") && (data[i]["Port"].ToString() != "0"))
                {
                    string dt_name = data[i]["Building"].ToString();
                    string ip = data[i]["IP_Address"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, IP_Address = ip,Port = port });
                }
            }

            return digitalTwinsList;
        }
        #endregion

    }
}
