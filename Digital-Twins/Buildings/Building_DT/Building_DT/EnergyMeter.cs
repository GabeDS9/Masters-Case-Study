using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Building_DT
{
    class EnergyMeter
    {
        // Energy Meter Data initialisation
        public string meterid { get; set; }
        public string description { get; set; }
        public string make { get; set; }
        public string type { get; set; }
        public string serial_no { get; set; }
        public string yard_no { get; set; }
        public string building_no { get; set; }
        public string floor { get; set; }
        public string room_no { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public List<EnergyData> data { get; set; }

        private APICaller apiCaller = new APICaller();

        private string prev_Date = "";

        // Get energy meter historic data
        public void GetEnergyUsage()
        {
            var apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
            var from_date = "2022-02-01%2000:00:00";
            var to_date = GetCurrentDateTime();
            prev_Date = to_date;
            var interval = "ts_5min";

            var url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval={interval}&key={apikey}";

            var task = apiCaller.CallEnergyAPI(url);

            if(task.Result != null)
            {
                EnergyMeter meterdata = JsonConvert.DeserializeObject<EnergyMeter>(task.Result);
                data = meterdata.data;

                if(data != null)
                {
                    if(data.Count > 0)
                    {
                        Console.WriteLine(meterdata.meterid + " " + meterdata.description + ": " + data[data.Count - 1].ptot_kw + " - Timestamp: " + data[data.Count - 1].timestamp);
                    }
                }
            }
            else
            {
                data = null;
                Console.WriteLine("No data");
            }
        }

        // Get current energy meter reading
        public void GetCurrentEnergyUsage()
        {
            var apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
            var from_date = prev_Date;
            var to_date = GetCurrentDateTime();

            var interval = "ts_5min";

            var url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval={interval}&key={apikey}";

            var task = apiCaller.CallEnergyAPI(url);

            EnergyMeter meterdata = JsonConvert.DeserializeObject<EnergyMeter>(task.Result);

            if (task != null && (meterdata.data.Count > 1))
            {
                if (data[data.Count - 1].timestamp != meterdata.data[meterdata.data.Count - 1].timestamp)
                {

                    data.Add(meterdata.data[meterdata.data.Count - 1]);
                    Console.WriteLine("Current Energy Data has been added to " + meterdata.description + " at " + data[data.Count - 1].timestamp);
                    prev_Date = to_date;
                }
                else
                {
                    Console.WriteLine("No current Energy Data has been added to " + meterdata.description);
                }
            }
            
        }

        // Gives current date and time for current meter readings
        private string GetCurrentDateTime()
        {
            string date = "";
            DateTime currDate = DateTime.Now;

            date = currDate.Year + "-" + currDate.Month + "-" + currDate.Day + "%20" + currDate.Hour + ":" + currDate.Minute + ":00";

            return date;
        }
    }
}
