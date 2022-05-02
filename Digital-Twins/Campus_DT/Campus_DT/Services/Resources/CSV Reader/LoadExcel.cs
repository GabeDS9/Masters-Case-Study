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

        private string DTConfigurationFile = "DTArchitectureConfiguration.csv";

        #region DigitalTwins
        public List<DigitalTwin> LoadDigitalTwins()
        {
            digitalTwinsList.Clear();

            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

            List<Dictionary<string, object>> data = CSVReader.Read(filepath);

            for (var i = 0; i < data.Count; i++)
            {
                // Add Campus DT
                if(data[i]["Precinct"].ToString() == "-")
                {
                    string dt_name = data[i]["Campus"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, Port = port });
                }
                else if((data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() == "-"))
                {
                    string dt_name = data[i]["Precinct"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, Port = port });
                }
                else if ((data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-"))
                {
                    string dt_name = data[i]["Reticulation"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, Port = port });
                }
                else if ((data[i]["Precinct"].ToString() != "-") && (data[i]["Building"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-"))
                {
                    string dt_name = data[i]["Building"].ToString();
                    int port = int.Parse(data[i]["Port"].ToString());

                    digitalTwinsList.Add(new DigitalTwin { DT_Name = dt_name, Port = port });
                }
            }

            return digitalTwinsList;
        }
        #endregion

    }
}
