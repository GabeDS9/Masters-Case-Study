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

    // Energy MeterUsage API Caller
    // This function will make an API call to receive an energy meter's information over a specified time period
    // Input: Energy Meter ID (string), time frame (string)
    // Output: Energy Data (MeterData)
    public async Task<List<EnergyData>> GetMeterDataAsync(String from_date, String to_date, int meterid)
    {
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval=ts_5min&key={apikey}";
        var result = await Task.Run(() => CallAPI(url));
        EnergyMeterData meterdata = JsonConvert.DeserializeObject<EnergyMeterData>(result);

        foreach (var item in EnergyMeters)
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

    // Store energy meter data
    // This function will make API calls to receive and store all energy information
    // Input: None
    // Output: Stored all energy meter data over time period
    /*public async Task<bool> StoreEnergyMeterDataAsync()
    {
        String to_date, from_date;
        (to_date, from_date) = GetCurrentDateTime();
        from_date = "2022-02-25%2000:00:00";
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
    }*/

    public void CalculateDayAverage(string startDate, string endDate, int meterid)
    {
        foreach (var item in EnergyMeters)
        {
            if (item.meterid == meterid)
            {
                item.data = GetMeterDataAsync(startDate, endDate, meterid).Result;
                item.day_average = new List<EnergyAverage>();

                int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
                int startDay, startMonth, startYear;
                (startDay, startMonth, startYear) = GetDate(startDate);
                int endDay, endMonth, endYear;
                (endDay, endMonth, endYear) = GetDate(endDate);
                double tempEnergy = 0;
                int count = 0;
                EnergyAverage tempData = new EnergyAverage();

                for (int i = 0; i < item.data.Count; i++)
                {
                    (year, month, day) = GetDate(item.data[i].timestamp);

                    if (((year != prevYear) || (month != prevMonth) || (day != prevDay)) && ((year >= startYear) || (month >= startMonth) || (day >= startDay))
                        && ((year <= endYear) || (month <= endMonth) || (day <= endDay)))
                    {
                        foreach (var data in item.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                            {
                                tempEnergy += data.ptot_kw;
                                count++;
                            }

                        }

                        tempData = new EnergyAverage();
                        tempData.timestamp = year + "-" + month + "-" + day;
                        tempData.ptot_kw = tempEnergy / count;
                        item.day_average.Add(tempData);
                        prevYear = year;
                        prevMonth = month;
                        prevDay = day;
                    }
                }
            }
        }
    }

    // Calculate monthly energy average
    // This function will calculate the average energy use for each month and store it in the energy meter
    // Input: None
    // Output: Stored all energy meter monthly average
    public void CalculateMonthAverage(string startDate, string endDate, int meterid)
    {
        // Loops through the list of energy meters
        foreach (var item in EnergyMeters)
        {
            if (item.meterid == meterid)
            {

                item.data = GetMeterDataAsync(startDate, endDate, meterid).Result;
                item.month_average = new List<EnergyAverage>();

                int day, month, year, prevMonth = 0, prevYear = 0;
                double tempEnergy = 0;
                int count = 0;
                int startDay, startMonth, startYear;
                (startDay, startMonth, startYear) = GetDate(startDate);
                int endDay, endMonth, endYear;
                (endDay, endMonth, endYear) = GetDate(endDate);
                EnergyAverage tempData = new EnergyAverage();

                for (int i = 0; i < item.day_average.Count; i++)
                {
                    (year, month, day) = GetDate(item.day_average[i].timestamp);

                    if ((year != prevYear) || (month != prevMonth) && ((year >= startYear) || (month >= startMonth))
                            && ((year <= endYear) || (month <= endMonth)))
                    {

                        foreach (var data in item.day_average)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth))
                            {
                                tempEnergy += data.ptot_kw;
                                count++;
                            }

                        }

                        tempData = new EnergyAverage();
                        tempData.timestamp = year + "-" + month + "-01";
                        tempData.ptot_kw = tempEnergy / count;
                        item.month_average.Add(tempData);
                        prevYear = year;
                        prevMonth = month;

                    }
                }
            }
        }
    }

    private float NextFloat(float min, float max)
    {
        double val = (random.NextDouble() * (max - min) + min);
        return (float)val;
    }

}


