using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Mapbox.Json;

public class EnergyAPIScript : APICaller
{
    // Energy MeterList API Caller
    // This function will make an API call to receive the energy meters availabe
    // Input: None
    // Output: Meter List (List<MeterList>)
    public static List<EnergyMeterList> GetEnergyMeterList()
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = "https://api.indivo.co.za/Energy/MeterList?key=" + apikey;

        List<EnergyMeterList> meterlist = JsonConvert.DeserializeObject<List<EnergyMeterList>>(CallAPI(url));

        // Temporary solution to remove meter 8656 because it returns an error when accessing it
        int pos = 0;
        foreach (var record in meterlist)
        {
            if (record.meterid == 8656)
            {
                // Get position of meter 8656 in the list
                pos = meterlist.IndexOf(record);
            }
            else
            {
                // Generate random coordinates until receive actual data with real coordinates
                record.longitude = (UnityEngine.Random.Range(18.8600f, 18.8800f)).ToString().Replace(',', '.');
                record.latitude = (UnityEngine.Random.Range(-33.9400f, -33.9600f)).ToString().Replace(',', '.');
            }
        }

        // Remove meter 8656
        meterlist.RemoveAt(pos);

        return meterlist;
    }

    // Energy MeterUsage API Caller
    // This function will make an API call to receive an energy meter's information over a specified time period
    // Input: Energy Meter ID (string), time frame (string)
    // Output: Energy Data (MeterData)
    public static EnergyMeterData GetMeterData(String from_date, String to_date, String meterid)
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";

        EnergyMeterData meterdata = JsonConvert.DeserializeObject<EnergyMeterData>(CallAPI(url));
        List<EnergyData> data = meterdata.data;

        return meterdata;
    }

    // Current Energy MeterUsage API Caller
    // This function will make an API call to receive an energy meter's current information
    // Input: Energy Meter ID (string)
    // Output: Current Energy Data (EnergyData)
    public static EnergyData GetCurrentMeterData(String meterid)
    {
        EnergyData temp = new EnergyData();
        String to_date, from_date;
        (to_date, from_date) = GetCurrentDateTime();
        EnergyMeterData meterData = GetMeterData(from_date, to_date, meterid);
        if (meterData.data.Count > 0)
        {
            temp = meterData.data[meterData.data.Count - 1];
        }

        return temp;
    }
}
