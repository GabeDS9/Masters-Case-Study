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
    private APICaller apiCaller = new APICaller();

    // Energy MeterUsage API Caller
    // This function will make an API call to receive an energy meter's information over a specified time period
    // Input: Energy Meter ID (string), time frame (string)
    // Output: Energy Data (MeterData)
    private async Task<List<EnergyData>> GetMeterDataAsync(String from_date, String to_date, int meterid)
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
                        tempEnergy += data.difference_imp_kwh;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / (count / 12));
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
                        tempEnergy += data.difference_imp_kwh;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / (count / 12));
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
                        tempEnergy += data.difference_imp_kwh;
                        count++;
                    }

                }
                dataList.Add(tempEnergy / (count/12));
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
            dataAverages = await CalculateDayAverageAsync(startDate, endDate, meterid);
        }
        else if (averageType == "Month")
        {
            dataAverages = await CalculateMonthAverageAsync(startDate, endDate, meterid);
        }
        else if (averageType == "Year")
        {
            dataAverages = await CalculateYearAverageAsync(startDate, endDate, meterid);
        }
        return dataAverages;
    }
    public async Task<List<double>> CalculateMaxesAsync(int meterid, string startDate, string endDate, string maxType)
    {
        List<double> dataMaxes = new List<double>();
        if (maxType == "Day")
        {
            dataMaxes = await CalculateDayMaxAsync(startDate, endDate, meterid);
        }
        else if (maxType == "Month")
        {
            dataMaxes = await CalculateMonthMaxAsync(startDate, endDate, meterid);
        }
        else if (maxType == "Year")
        {
            dataMaxes = await CalculateYearMaxAsync(startDate, endDate, meterid);
        }
        return dataMaxes;
    }
    private async Task<List<double>> CalculateDayMaxAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        double maxEnergy = 0;

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
                        if(data.ptot_kw > maxEnergy)
                        {
                            maxEnergy = data.ptot_kw;
                        }
                    }

                }
                dataList.Add(maxEnergy);
                maxEnergy = 0;
                prevYear = year;
                prevMonth = month;
                prevDay = day;
            }
        }
        return dataList;
    }
    private async Task<List<double>> CalculateMonthMaxAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year, prevMonth = 0, prevYear = 0;
        double maxEnergy = 0;
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
                        if (data.ptot_kw > maxEnergy)
                        {
                            maxEnergy = data.ptot_kw;
                        }
                    }
                }
                dataList.Add(maxEnergy);
                maxEnergy = 0;
                prevYear = year;
                prevMonth = month;
            }
        }
        return dataList;
    }
    private async Task<List<double>> CalculateYearMaxAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year, prevYear = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        EnergyAverage tempData = new EnergyAverage();
        double maxEnergy = 0;

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].timestamp);

            if ((year != prevYear) && (year >= startYear) && (year <= endYear))
            {

                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                    if ((year == tempYear) && (month == tempMonth))
                    {
                        if (data.ptot_kw > maxEnergy)
                        {
                            maxEnergy = data.ptot_kw;
                        }
                    }
                }
                dataList.Add(maxEnergy);
                maxEnergy = 0;
                prevYear = year;
            }
        }
        return dataList;
    }
    public async Task<List<double>> CalculateTotalAsync(int meterid, string startDate, string endDate, string maxType)
    {
        List<double> dataMaxes = new List<double>();
        if (maxType == "Day")
        {
            dataMaxes = await CalculateDayTotalAsync(startDate, endDate, meterid);
        }
        else if (maxType == "Month")
        {
            dataMaxes = await CalculateMonthTotalAsync(startDate, endDate, meterid);
        }
        else if (maxType == "Year")
        {
            dataMaxes = await CalculateYearTotalAsync(startDate, endDate, meterid);
        }
        return dataMaxes;
    }
    private async Task<List<double>> CalculateDayTotalAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        double totalEnergy = 0;

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
                        totalEnergy += data.difference_imp_kwh;
                    }

                }
                dataList.Add(totalEnergy);
                totalEnergy = 0;
                prevYear = year;
                prevMonth = month;
                prevDay = day;
            }
        }
        return dataList;
    }
    private async Task<List<double>> CalculateMonthTotalAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year, prevMonth = 0, prevYear = 0;
        double totalEnergy = 0;
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
                        totalEnergy += data.difference_imp_kwh;
                    }
                }
                dataList.Add(totalEnergy);
                totalEnergy = 0;
                prevYear = year;
                prevMonth = month;
            }
        }
        return dataList;
    }
    private async Task<List<double>> CalculateYearTotalAsync(string startDate, string endDate, int meterid)
    {
        List<double> dataList = new List<double>();
        var meterData = await GetMeterDataAsync(startDate, endDate, meterid);

        int day, month, year, prevYear = 0;
        int startDay, startMonth, startYear;
        (startYear, startMonth, startDay) = GetDate(startDate);
        int endDay, endMonth, endYear;
        (endYear, endMonth, endDay) = GetDate(endDate);
        EnergyAverage tempData = new EnergyAverage();
        double totalEnergy = 0;

        for (int i = 0; i < meterData.Count; i++)
        {
            (year, month, day) = GetDate(meterData[i].timestamp);

            if ((year != prevYear) && (year >= startYear) && (year <= endYear))
            {

                foreach (var data in meterData)
                {
                    int tempDay, tempMonth, tempYear;
                    (tempYear, tempMonth, tempDay) = GetDate(data.timestamp);
                    if ((year == tempYear) && (month == tempMonth))
                    {
                        totalEnergy += data.difference_imp_kwh;
                    }
                }
                dataList.Add(totalEnergy);
                totalEnergy = 0;
                prevYear = year;
            }
        }
        return dataList;
    }
}


