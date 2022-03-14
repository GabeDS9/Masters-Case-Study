using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Mapbox.Json;

public class WaterAPIScript : APICaller
{
    // Water MeterList API Caller
    // This function will make an API call to receive the Water meters availabe
    // Input: None
    // Output: Meter List (List<MeterList>)
    public static List<WaterMeterList> GetWaterMeterList()
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = "https://api.indivo.co.za/Energy/MeterList?key=" + apikey;

        List<WaterMeterList> meterlist = JsonConvert.DeserializeObject<List<WaterMeterList>>(CallAPI(url));

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

    // Water MeterUsage API Caller
    // This function will make an API call to receive an Water meter's information over a specified time period
    // Input: Water Meter ID (string), time frame (string)
    // Output: Water Data (MeterData)
    public static WaterMeterData GetMeterData(String from_date, String to_date, String meterid)
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";

        WaterMeterData meterdata = JsonConvert.DeserializeObject<WaterMeterData>(CallAPI(url));
        List<WaterData> data = meterdata.data;

        return meterdata;
    }

    // Current Water MeterUsage API Caller
    // This function will make an API call to receive an Water meter's current information
    // Input: Water Meter ID (string)
    // Output: Current Water Data (WaterData)
    public static WaterData GetCurrentMeterData(String meterid)
    {
        WaterData temp = new WaterData();
        String to_date, from_date;
        (to_date, from_date) = GetCurrentDateTime();
        WaterMeterData meterData = GetMeterData(from_date, to_date, meterid);
        if(meterData.data.Count > 0)
        {
            temp = meterData.data[meterData.data.Count - 1];
        }

        return temp;
    }
}
