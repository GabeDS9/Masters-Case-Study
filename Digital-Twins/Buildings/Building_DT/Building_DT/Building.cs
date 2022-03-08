using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Building_DT
{    class Building
    {
        // Intialise building specific info
        public string Name { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public List<EnergyMeter> EnergyMeters { get; set; }

        private APICaller apiCaller = new APICaller();

        public Building(string name, int latitude, int longitude, List<EnergyMeter> energyMeterList)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            EnergyMeters = energyMeterList;
        }

        // Get available energy meters in the building
        public void GetEnergyMeterList()
        {
            var apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";

            var url = $"https://api.indivo.co.za/Energy/MeterList?key={apikey}";

            var task = apiCaller.CallEnergyAPI(url);

            EnergyMeters = JsonConvert.DeserializeObject<List<EnergyMeter>>(task.Result);
            int pos = 0;

            foreach(var record in EnergyMeters)
            {
                if(record.meterid == "8656")
                {
                    pos = EnergyMeters.IndexOf(record);
                }
            }

            EnergyMeters.RemoveAt(pos);

            foreach(var record in EnergyMeters)
            {
                record.GetEnergyUsage();
            }
        }

        public void SendMeterList(SocketServer server)
        {
            foreach(var record in EnergyMeters)
            {
                server.SendMessage(record.description);
                string temp = server.ReceiveMessage();

                Console.WriteLine(temp);
            }

            if(server.ReceiveMessage() == "<EOF>")
            {
                server.SendMessage("<EOF>");
                server.CloseServer();
            }
            
        }

        // Get current energy meter reading
        public void GetCurrentMeterReadings()
        {
            foreach (var record in EnergyMeters)
            {

                record.GetCurrentEnergyUsage();

                if (record.data.Count > 0)
                {
                    Console.WriteLine("Current Usage for " + record.meterid + " " + record.description + ": " + record.data[record.data.Count - 1].ptot_kw + " - Timestamp: " + record.data[record.data.Count - 1].timestamp);

                }
            }
        }
    }
}
