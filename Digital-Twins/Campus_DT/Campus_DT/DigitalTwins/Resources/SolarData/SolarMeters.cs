using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

    public class SolarMeters
    {
        public List<SolarMeterData> solarMeters = new List<SolarMeterData>();

        private APICaller apiCaller = new APICaller();

        // Load Energy Meter List
        // This function will populate a energy meter list with data froma CSV configuration file for the energy meters
        // Input: None
        // Output: Energy Meter List (List<EnergyMeterData>)
        public List<SolarMeterData> LoadSolarMeterList(string buildRecName)
        {
            LoadExcel excel = new LoadExcel();

            solarMeters = excel.LoadSolarMeterData(buildRecName);

            return solarMeters;
        }

        public List<SolarData> GetMeterData(String from_date, String to_date, int meterid)
        {
            String apikey = "fe418d6e0eec489b0bb7bb2f16f1e8af802b1d7195a0f00be107d7c5bafb5a2c"; //"[YOUR API KEY HERE]";
            String url = $"https://api.indivo.co.za/Solar/SolarData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";
            var result = apiCaller.CallAPI(url);
            SolarMeterData meterdata = JsonConvert.DeserializeObject<SolarMeterData>(result);

            foreach (var item in solarMeters)
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
