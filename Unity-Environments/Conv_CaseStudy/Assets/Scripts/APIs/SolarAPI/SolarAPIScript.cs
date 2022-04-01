using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Mapbox.Json;

public class SolarAPIScript : APICaller
{
    public List<SolarMeterData> SolarMeters = new List<SolarMeterData>();

    // Load Energy Meter List
    // This function will populate a energy meter list with data froma CSV configuration file for the energy meters
    // Input: None
    // Output: Energy Meter List (List<EnergyMeterData>)
    public List<SolarMeterData> LoadSolarMeterList()
    {
        LoadExcel excel = new LoadExcel();

        SolarMeters = excel.LoadSolarMeterData();

        return SolarMeters;
    }

    public async Task<List<SolarData>> GetMeterDataAsync(String from_date, String to_date, int meterid)
    {
        String apikey = "fe418d6e0eec489b0bb7bb2f16f1e8af802b1d7195a0f00be107d7c5bafb5a2c"; //"[YOUR API KEY HERE]";
        String url = $"https://api.indivo.co.za/Solar/SolarData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";
        var result = await Task.Run(() => CallAPI(url));
        SolarMeterData meterdata = JsonConvert.DeserializeObject<SolarMeterData>(result);

        foreach (var item in SolarMeters)
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
