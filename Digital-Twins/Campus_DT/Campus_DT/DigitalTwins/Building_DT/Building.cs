using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using DataAccess;
using DataAccess.Models;
using Newtonsoft.Json;

namespace Building_DT
{
    public class Building
    {
        // Intialise building specific info
        public string Building_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int Port { get; set; }
        public List<EnergyMeterData> EnergyMeters { get; set; }
        public List<OccupancyMeterData> OccupancyMeters { get; set; }
        public List<SolarMeterData> SolarMeters { get; set; }

        private EnergyMeters energyManager = new EnergyMeters();
        private OccupancyMeters occupancyManager = new OccupancyMeters();
        private SolarMeters solarManager = new SolarMeters();
        private APICaller apiCaller = new APICaller();
        private Stopwatch stopWatch = new Stopwatch();
        private BuildingDBDataAccess db;

        private string startingDate = "2022-04-20 00:00:00";

        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        Services_Communication.ServerSocket myServer = new Services_Communication.ServerSocket();

        public Building(string name, string latitude, string longitude, int port)
        {
            Building_name = name;
            Latitude = latitude;
            Longitude = longitude;
            Port = port;
            db = new BuildingDBDataAccess(Building_name.Replace(" ", "_"));
            _ = InitialiseBuildingAsync();
        }

        # region Initialisation Functions
        // Get available energy meters in the building
        public async Task InitialiseBuildingAsync()
        {
            EnergyMeters = energyManager.LoadEnergyMeterList(Building_name);
            var energymeters = new List<EnergyMeterModel>();

            foreach (var item in EnergyMeters)
            {
                var tempMeter = new EnergyMeterModel(item.description, item.meterid, 0, null);
                energymeters.Add(tempMeter);
                await db.CreateEnergyMeter(tempMeter);
            }

            OccupancyMeters = occupancyManager.LoadOccupancyMeterList(Building_name);
            SolarMeters = solarManager.LoadSolarMeterList(Building_name);

            var building = new BuildingModel(Building_name, Latitude, Longitude, energymeters, OccupancyMeters, SolarMeters);
            myServer.SetupServer(Port, this, null, null);
            await db.CreateBuilding(building);
            await Task.Run(() => InitialPopulateDataBase());
            await Task.Run(() => RunBuildingDTAsync());
        }

        private void InitialPopulateDataBase()
        {
            InitialiseMeterData();
            InitialContextGeneration(startingDate, apiCaller.GetCurrentDateTime().Item1);
            _ = SaveToDataBaseInitialAsync();
        }

        private void InitialiseMeterData()
        {
            foreach (var item in EnergyMeters)
            {
                item.data = energyManager.GetMeterData(startingDate, apiCaller.GetCurrentDateTime().Item1, item.meterid);
                item.latest_power = item.data[item.data.Count - 1].ptot_kw;
                item.latest_timestamp = item.data[item.data.Count - 1].timestamp;
            }
        }

        private void InitialContextGeneration(String startDate, String endDate)
        {
            CalculateInitialDayAverage(startDate, endDate);
            //CalculateMonthAverage();
        }

        public void CalculateInitialDayAverage(string startDate, string endDate)
        {
            foreach (var item in EnergyMeters)
            {
                item.day_average = new List<EnergyAverageData>();

                int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
                int startDay, startMonth, startYear;
                (startDay, startMonth, startYear) = apiCaller.GetDate(startDate);
                int endDay, endMonth, endYear;
                (endDay, endMonth, endYear) = apiCaller.GetDate(endDate);
                double tempEnergy = 0;
                int count = 0;
                EnergyAverageData tempData = new EnergyAverageData();

                for (int i = 0; i < item.data.Count; i++)
                {
                    (year, month, day) = apiCaller.GetDate(item.data[i].timestamp);

                    if (((year != prevYear) || (month != prevMonth) || (day != prevDay)) && ((year >= startYear) || (month >= startMonth) || (day >= startDay))
                        && ((year <= endYear) || (month <= endMonth) || (day <= endDay)))
                    {
                        foreach (var data in item.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = apiCaller.GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                            {
                                tempEnergy += data.ptot_kw;
                                count++;
                            }

                        }

                        tempData = new EnergyAverageData();
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

        private async Task SaveToDataBaseInitialAsync()
        {
            // Save Energy meter day averages to database
            foreach (var item in EnergyMeters)
            {
                for (int i = 0; i < item.day_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, item.day_average[i].ptot_kw, item.day_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
            }
            Console.WriteLine($"{Building_name} DT has been initialised");
        }
        #endregion

        public async Task RunBuildingDTAsync()
        {
            stopWatch.Start();
            while (true)
            {
                double ts = stopWatch.Elapsed.TotalSeconds;
                if (ts >= 5)
                {

                    await GetCurrentMeterDataAsync();
                    stopWatch.Restart();
                }
            }
        }
        
        public async Task<string> AccessDatabaseAsync(int meterid, string date)
        {
            var temp = await db.GetEnergyMeterReading(meterid, date);
            var info = JsonConvert.SerializeObject(temp[0]);
            return info;
        }

        private async Task GetCurrentMeterDataAsync()
        {
            foreach (var item in EnergyMeters)
            {
                var tempData = energyManager.GetCurrentEnergyData(item.meterid);
                if (tempData.Count > 0)
                {
                    if (tempData[tempData.Count - 1].timestamp != item.latest_timestamp)
                    {
                        item.previous_timestamp = item.latest_timestamp;
                        item.latest_timestamp = tempData[tempData.Count - 1].timestamp;
                        item.latest_power = tempData[tempData.Count - 1].ptot_kw;
                        var newEnergyData = await CalculateDayAverageAsync(item.meterid, item.latest_power, item.previous_timestamp);
                        await db.UpdateEnergyMeter(newEnergyData);
                        //Console.WriteLine($"New data available for {item.meterid} - {item.description}");
                    }
                }
            }
        }

        #region Services
        private void ContextGeneration(String currDate)
        {
            //CalculateDayAverage(currDate);
            Console.WriteLine("Average Energy Reading for " + EnergyMeters[0].description + " was " + EnergyMeters[0].day_average[0].timestamp + " - " + EnergyMeters[0].day_average[0].ptot_kw);
            //CalculateMonthAverage();
        }

        public async Task<EnergyMeterModel> CalculateDayAverageAsync(int meterid, double newPower, string prevTime)
        {
            var prevTimestamp = DecodeTimestamp(prevTime, "Day");
            var temp = await db.GetEnergyMeterReading(meterid, prevTimestamp);
            var newp = (temp[0].Power_Tot + newPower) / 2;

            temp[0].Power_Tot = newp;

            return temp[0];
        }

        private string DecodeTimestamp(string timestamp, string type)
        {
            string newTime = "";

            if (type == "Day")
            {
                String[] temp = timestamp.Split('-');

                String yr = temp[0];
                String mon = temp[1];
                String dy = temp[2];

                if (dy.Length > 2)
                {
                    String[] daytemp = dy.Split(' ');
                    dy = daytemp[0];
                    if (dy[0].Equals("0"))
                    {
                        dy = dy[1].ToString();
                    }
                }
                if (mon[0].Equals('0'))
                {
                    mon = mon[1].ToString();
                }


                newTime = yr + "-" + mon + "-" + dy;
            }

            return newTime;
        }

        /*
        public void CalculateDayAverage(int meterid)
        {
            double prevPtot = AccessDatabase();
            String prevDate = "";

            foreach (var item in EnergyMeters)
            {
                item.day_average = new List<EnergyAverageData>();

                int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
                int currDay, currMonth, currYear;
                (currDay, currMonth, currYear) = apiCaller.GetDate(currDate);
                double tempEnergy = 0;
                int count = 0;
                EnergyAverageData tempData = new EnergyAverageData();

                for (int i = 0; i < item.data.Count; i++)
                {
                    (year, month, day) = apiCaller.GetDate(prevDate);

                    if (((year != prevYear) || (month != prevMonth) || (day != prevDay)) && ((year >= startYear) || (month >= startMonth) || (day >= startDay))
                        && ((year <= endYear) || (month <= endMonth) || (day <= endDay)))
                    {
                        foreach (var data in item.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = apiCaller.GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                            {
                                tempEnergy += data.ptot_kw;
                                count++;
                            }

                        }

                        tempData = new EnergyAverageData();
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

        /*
        // Calculate monthly energy average
        // This function will calculate the average energy use for each month and store it in the energy meter
        // Input: None
        // Output: Stored all energy meter monthly average
        /*public void CalculateMonthAverage(string startDate, string endDate, int meterid)
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
        }*/
        #endregion
    }
}

