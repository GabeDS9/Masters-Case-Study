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
    public List<EnergyMeterData> EnergyMeters = new List<EnergyMeterData>();
    public static System.Random random = new System.Random();

    // Initialise the energy meters
    // This function will initialise the energy meters with their historic information
    // Input: None
    // Output: Stored all energy meter data over time period
    public async Task<bool> InitialiseEnergyMetersAsync()
    {
        EnergyMeters = GetEnergyMeterListAsync();

        Debug.Log("Populated energy meters");

        var result = await StoreEnergyMeterDataAsync();

        await Task.Run(() => CalculateDayAverage());

        await Task.Run(() => CalculateMonthAverage());

        foreach (var item in EnergyMeters)
        {
            Debug.Log($"{item.meterid} has day average of {item.day_average[3].ptot_kw} on {item.day_average[3].timestamp}");
        }        

        foreach (var item in EnergyMeters)
        {
            Debug.Log($"{item.meterid} has month average of {item.month_average[0].ptot_kw} on {item.month_average[0].timestamp}");
        }

        Debug.Log("Energy meters initialised");
        return true;
    }

    // Energy MeterList API Caller
    // This function will make an API call to receive the energy meters availabe
    // Input: None
    // Output: Meter List (List<MeterList>)
    public List<EnergyMeterData> GetEnergyMeterListAsync()
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = "https://api.indivo.co.za/Energy/MeterList?key=" + apikey;
        var result = Task.Run(() => CallAPI(url));
        List<EnergyMeterData> meterlist = JsonConvert.DeserializeObject<List<EnergyMeterData>>(result.Result);

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
                record.longitude = NextFloat(18.8600f, 18.8800f).ToString().Replace(',', '.');
                record.latitude = NextFloat(-33.9400f, -33.9600f).ToString().Replace(',', '.');
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
    public static async Task<EnergyMeterData> GetMeterDataAsync(String from_date, String to_date, int meterid)
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";
        var result = await Task.Run(() => CallAPI(url));
        EnergyMeterData meterdata = JsonConvert.DeserializeObject<EnergyMeterData>(result);

        return meterdata;
    }

    // Current Energy MeterUsage API Caller
    // This function will make an API call to receive an energy meter's current information
    // Input: Energy Meter ID (string)
    // Output: Current Energy Data (EnergyData)
    public static EnergyMeterData GetCurrentMeterData(int meterid)
    {
        EnergyData temp = new EnergyData();
        String to_date, from_date;
        (to_date, from_date) = GetCurrentDateTime();
        EnergyMeterData meterData = GetMeterDataAsync(from_date, to_date, meterid).Result;
        if (meterData.data.Count > 0)
        {
            temp = meterData.data[meterData.data.Count - 1];
        }

        return meterData;
    }

    // Store energy meter data
    // This function will make API calls to receive and store all energy information
    // Input: None
    // Output: Stored all energy meter data over time period
    public async Task<bool> StoreEnergyMeterDataAsync()
    {
        String to_date, from_date;
        (to_date, from_date) = GetCurrentDateTime();
        from_date = "2022-02-27%2000:00:00";
        List<Task<EnergyMeterData>> tasks = new List<Task<EnergyMeterData>>();

        foreach (var record in EnergyMeters)
        {
            tasks.Add(Task.Run(() => GetMeterDataAsync(from_date, to_date, record.meterid)));
        }

        var results = await Task.WhenAll(tasks);

        foreach(var record in results)
        {
            foreach(var item in EnergyMeters)
            {
                if(record.meterid == item.meterid)
                {
                    EnergyMeters[EnergyMeters.IndexOf(item)].data = record.data;
                    break;
                }
            }
        }

        Debug.Log("Energy Meters Stored");

        if(results != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Calculate daily energy average
    // This function will calculate the average energy use for each day and store it in the energy meter
    // Input: None
    // Output: Stored all energy meter daily average
    private void CalculateDayAverage()
    {
        // Loops through the list of energy meters
        foreach(var item in EnergyMeters)
        {
            item.day_average = new List<EnergyAverage>();

            String day, month, year, prevDay = "", prevMonth = "", prevYear = "";
            double tempEnergy = 0;
            int count = 0;
            EnergyAverage tempData = new EnergyAverage();

            for (int i = 0; i < item.data.Count; i++)
            {
            (year, month, day) = GetDate(item.data[i].timestamp);
                    //Debug.Log($"Current time: {year}-{month}-{day}");

                    if ((year != prevYear) || (month != prevMonth) || (day != prevDay))
                    {
                        //Debug.Log($"Timestamp is not equal: {year}-{month}-{day} is current -- {prevYear}-{prevMonth}-{prevDay} is prev");
                        
                        foreach (var data in item.data)
                        {
                            String tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                            {
                                tempEnergy += data.ptot_kw;
                                count++;
                            }

                        }

                        //Debug.Log($"{year}-{month}-{day} has average energy of {tempEnergy/count}");
                        tempData.timestamp = year + "-" + month + "-" + day;
                        tempData.ptot_kw = tempEnergy / count;
                        item.day_average.Add(tempData);
                        //Debug.Log($"{item.meterid} average energy for {tempData.timestamp} was {tempData.ptot_kw}");
                        prevYear = year;
                        prevMonth = month;
                        prevDay = day;
                    }
                }
            }
    }

    // Calculate monthly energy average
    // This function will calculate the average energy use for each month and store it in the energy meter
    // Input: None
    // Output: Stored all energy meter monthly average
    private void CalculateMonthAverage()
    {
        
        // Loops through the list of energy meters
        foreach (var item in EnergyMeters)
        {
            item.month_average = new List<EnergyAverage>();

            String day, month, year, prevMonth = "", prevYear = "";
            double tempEnergy = 0;
            int count = 0;
            EnergyAverage tempData = new EnergyAverage();

            for (int i = 0; i < item.day_average.Count; i++)
            {
                (year, month, day) = GetDate(item.day_average[i].timestamp);
                //Debug.Log($"Current time: {year}-{month}-{day}");

                if ((year != prevYear) || (month != prevMonth))
                {
                    //Debug.Log($"Timestamp is not equal: {year}-{month}-{day} is current -- {prevYear}-{prevMonth}-01 is prev for {item.meterid}");

                    foreach (var data in item.day_average)
                    {
                        String tempDay, tempMonth, tempYear;
                        (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                        if ((year == tempYear) && (month == tempMonth))
                        {
                            tempEnergy += data.ptot_kw;
                            count++;
                        }

                    }

                    //Debug.Log($"{year}-{month}-{day} has average energy of {tempEnergy/count}");
                    tempData.timestamp = year + "-" + month + "-01";
                    tempData.ptot_kw = tempEnergy / count;
                    item.month_average.Add(tempData);
                    //Debug.Log($"{item.meterid} average energy for {tempData.timestamp} was {tempData.ptot_kw}");
                    prevYear = year;
                    prevMonth = month;
                }
            }
        }
    }

    public EnergyMeterData ReturnEnergyMeterData(int meterid)
    {
        EnergyMeterData energyMeter = new EnergyMeterData();

        foreach(var item in EnergyMeters)
        {
            if(item.meterid == meterid)
            {
                energyMeter = item;
                break;
            }
        }

        return energyMeter;
    }

    private float NextFloat(float min, float max)
    {
        double val = (random.NextDouble() * (max - min) + min);
        return (float)val;
    }

}


