using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

    public class OccupancyMeters
    {
        public List<OccupancyMeterData> occupancyMeters = new List<OccupancyMeterData>();

        private APICaller apiCaller = new APICaller();

        // Load Energy Meter List
        // This function will populate a energy meter list with data froma CSV configuration file for the energy meters
        // Input: None
        // Output: Energy Meter List (List<EnergyMeterData>)
        public List<OccupancyMeterData> LoadOccupancyMeterList(string building)
        {
            LoadExcel excel = new LoadExcel();

            occupancyMeters = excel.LoadOccupancyMeterData(building);

            return occupancyMeters;
        }

        // Energy MeterUsage API Caller
        // This function will make an API call to receive an energy meter's information over a specified time period
        // Input: Energy Meter ID (string), time frame (string)
        // Output: Energy Data (MeterData)
        public List<OccupancyData> GetMeterData(String from_date, String to_date, int meterid)
        {
            String apikey = "50cda6cd3da511eee65f84d1bca12eb46bf5e5dfc184c281913e62901eb5ab62"; //"[YOUR API KEY HERE]";
            String url = $"https://api.indivo.co.za/Occupancy/OccupancyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";
            var result = apiCaller.CallAPI(url);
            OccupancyMeterData meterdata = JsonConvert.DeserializeObject<OccupancyMeterData>(result);

            foreach (var item in occupancyMeters)
            {
                if (item.meterid == meterid)
                {
                    item.data = meterdata.data;
                    meterdata = item;
                    break;
                }
            }

            return meterdata.data;
        }
    }
