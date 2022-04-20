using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class EnergyMeters
{
    public List<EnergyMeterData> energyMeters = new List<EnergyMeterData>();
    private APICaller apiCaller = new APICaller();

    // Load Energy Meter List
    // This function will populate a energy meter list with data froma CSV configuration file for the energy meters
    // Input: None
    // Output: Energy Meter List (List<EnergyMeterData>)
    public List<EnergyMeterData> LoadEnergyMeterList(string buildRecName)
    {
        LoadExcel excel = new LoadExcel();

        energyMeters = excel.LoadEnergyMeterData(buildRecName);

        return energyMeters;
    }

    // Energy MeterUsage API Caller
    // This function will make an API call to receive an energy meter's information over a specified time period
    // Input: Energy Meter ID (string), time frame (string)
    // Output: Energy Data (MeterData)
    public List<EnergyData> GetMeterData(String from_date, String to_date, int meterid)
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";
        var result = apiCaller.CallAPI(url);
        EnergyMeterData meterdata = JsonConvert.DeserializeObject<EnergyMeterData>(result);

        foreach (var item in energyMeters)
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

    public List<EnergyData> GetCurrentEnergyData(int meterid)
    {
        String to_date, from_date;
        (to_date, from_date) = apiCaller.GetCurrentDateTime();

        return GetMeterData(from_date, to_date, meterid);
    }
}


