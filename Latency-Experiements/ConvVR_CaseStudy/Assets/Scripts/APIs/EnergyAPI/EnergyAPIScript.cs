using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Mapbox.Json;
using Utils;
using DataAccess;
using DataModels;

public class EnergyAPIScript : APICaller
{
    public List<EnergyMeterData> EnergyMeters = new List<EnergyMeterData>();
    private APICaller apiCaller = new APICaller();
    private Utilities utilities = new Utilities();
    private DBDataAccess db = new DBDataAccess();
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
        return meterdata.data;
    }
    public async Task<List<EnergyData>> GetCurrentEnergyDataAsync(int meterid)
    {
        String to_date, from_date;
        (to_date, from_date) = apiCaller.GetCurrentDateTime();

        return await GetMeterDataAsync(from_date, to_date, meterid);
    }
    private async Task<List<double>> CalculateDayAverageAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);
        int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        double tempEnergy = 0;
        int count = 0;

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].timestamp);

            if (((year != prevYear) || (month != prevMonth) || (day != prevDay)) && ((year >= startYear) || (month >= startMonth) || (day >= startDay))
                && ((year <= endYear) || (month <= endMonth) || (day <= endDay)))
            {
                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                    if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                    {
                        tempEnergy += data.ptot_kw;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / count);
                prevYear = year;
                prevMonth = month;
                prevDay = day;
            }
        }

        return dataList;
    }
    private async Task<List<double>> CalculateMonthAverageAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year, prevMonth = 0, prevYear = 0;
        double tempEnergy = 0;
        int count = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        EnergyAverage tempData = new EnergyAverage();

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].timestamp);

            if ((year != prevYear) || (month != prevMonth) && ((year >= startYear) || (month >= startMonth))
                    && ((year <= endYear) || (month <= endMonth)))
            {

                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                    if ((year == tempYear) && (month == tempMonth))
                    {
                        tempEnergy += data.ptot_kw;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / count);
                prevYear = year;
                prevMonth = month;
            }
        }
        return dataList;
    }
    private async Task<List<double>> CalculateYearAverageAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year,  prevYear = 0;
        double tempEnergy = 0;
        int count = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        EnergyAverage tempData = new EnergyAverage();

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].timestamp);

            if ((year != prevYear)  && (year >= startYear) && (year <= endYear))
            {

                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                    if ((year == tempYear) && (month == tempMonth))
                    {
                        tempEnergy += data.ptot_kw;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / count);
                prevYear = year;
            }
        }
        return dataList;
    }
    public async Task<List<double>> CalculateAveragesAsync(int meterid, string startDate, string endDate, string averageType)
    {
        List<double> dataAverages = new List<double>();
        if (averageType == "Day")
        {
            //dataAverages = await CalculateDayAverageAsync(startDate, endDate, meterid);
            dataAverages = await CalculateLocalDayAverageAsync(startDate, endDate, meterid);
        }
        else if (averageType == "Month")
        {
            //dataAverages = await CalculateMonthAverageAsync(startDate, endDate, meterid);
            dataAverages = await CalculateLocalMonthAverageAsync(startDate, endDate, meterid);
        }
        else if (averageType == "Year")
        {
            //dataAverages = await CalculateYearAverageAsync(startDate, endDate, meterid);
            dataAverages = await CalculateLocalYearAverageAsync(startDate, endDate, meterid);
        }
        return dataAverages;
    }
    private async Task<List<double>> CalculateLocalDayAverageAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var dates = utilities.GenerateDateList(startDate, endDate, "Day");
        List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
        foreach(var date in dates)
        {
            string tempDate = utilities.ChangeDateFormat(date);
            var temp = await db.GetEnergyMeterReading(meterid, tempDate);
            foreach(var data in temp)
            {
                meterData.Add(data);
            }
        }        
        int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        double tempEnergy = 0;
        int count = 0;

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].TimestampDay);

            if (((year != prevYear) || (month != prevMonth) || (day != prevDay)) && ((year >= startYear) || (month >= startMonth) || (day >= startDay))
                && ((year <= endYear) || (month <= endMonth) || (day <= endDay)))
            {
                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.TimestampDay);
                    if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                    {
                        tempEnergy += data.Power_Diff;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / (count/12));
                prevYear = year;
                prevMonth = month;
                prevDay = day;
            }
        }

        return dataList;
    }
    private async Task<List<double>> CalculateLocalMonthAverageAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var dates = utilities.GenerateDateList(startDate, endDate, "Day");
        List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
        foreach (var date in dates)
        {
            var temp = await db.GetEnergyMeterReading(meterid, date);
            foreach (var data in temp)
            {
                meterData.Add(data);
            }
        }
        int day, month, year, prevMonth = 0, prevYear = 0;
        double tempEnergy = 0;
        int count = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        EnergyAverage tempData = new EnergyAverage();

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].TimestampDay);

            if ((year != prevYear) || (month != prevMonth) && ((year >= startYear) || (month >= startMonth))
                    && ((year <= endYear) || (month <= endMonth)))
            {

                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.TimestampDay);
                    if ((year == tempYear) && (month == tempMonth))
                    {
                        tempEnergy += data.Power_Diff;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / (count/12));
                prevYear = year;
                prevMonth = month;
            }
        }
        return dataList;
    }
    private async Task<List<double>> CalculateLocalYearAverageAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var dates = utilities.GenerateDateList(startDate, endDate, "Day");
        List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
        foreach (var date in dates)
        {
            var temp = await db.GetEnergyMeterReading(meterid, date);
            foreach (var data in temp)
            {
                meterData.Add(data);
            }
        }
        int day, month, year, prevYear = 0;
        double tempEnergy = 0;
        int count = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        EnergyAverage tempData = new EnergyAverage();

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].TimestampDay);

            if ((year != prevYear) && (year >= startYear) && (year <= endYear))
            {

                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.TimestampDay);
                    if ((year == tempYear) && (month == tempMonth))
                    {
                        tempEnergy += data.Power_Diff;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / (count/12));
                prevYear = year;
            }
        }
        return dataList;
    }
}


