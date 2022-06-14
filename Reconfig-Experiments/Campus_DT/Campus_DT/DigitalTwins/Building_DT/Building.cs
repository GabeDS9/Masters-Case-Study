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
using Models;
using Utils;

namespace Building_DT
{
    public class Building
    {
        // Intialise building specific info
        public string Building_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string IP_Address { get; set; }
        public int Port { get; set; }
        public List<EnergyMeterData> EnergyMeters { get; set; }
        public List<OccupancyMeterData> OccupancyMeters { get; set; }
        public List<SolarMeterData> SolarMeters { get; set; }

        public EnergyMeterData buildingEnergyMeter = new EnergyMeterData();

        public bool Initialised = false;
        public bool NewEnergyDataAvailable = false;

        private EnergyMeters energyManager = new EnergyMeters();
        private OccupancyMeters occupancyManager = new OccupancyMeters();
        private SolarMeters solarManager = new SolarMeters();
        private APICaller apiCaller = new APICaller();
        private Stopwatch stopWatch = new Stopwatch();
        private BuildingDBDataAccess db;
        private Utilities utilities = new Utilities();
        private string startingDate;
        private double latestBuildingEnergy = 0;
        private double mainLatestBuildingEnergy = 0;

        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        Services_Communication.ServerSocket myServer = new Services_Communication.ServerSocket();

        public Building(string name, string latitude, string longitude, string ipAdd, int port, string iniDate)
        {
            Building_name = name;
            Latitude = latitude;
            Longitude = longitude;
            IP_Address = ipAdd;
            Port = port;
            db = new BuildingDBDataAccess(Building_name.Replace(" ", "_"));
            _ = db.DeleteDatabase(Building_name.Replace(" ", "_"));
            startingDate = iniDate;
            //_ = InitialiseBuildingAsync();
        }

        # region Initialisation Functions
        // Get available energy meters in the building
        public void InitialiseBuilding()
        {
            try
            {
                Console.WriteLine("Initialising " + Building_name);
                myServer.SetupServer(Port, this, null, null);
                EnergyMeters = energyManager.LoadEnergyMeterList(Building_name);
                var energymeters = new List<EnergyMeterModel>();
                if (EnergyMeters.Count > 1)
                {
                    buildingEnergyMeter = EnergyMeters[0];
                    EnergyMeters.RemoveAt(0);
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Meter", buildingEnergyMeter.latitude, buildingEnergyMeter.longitude, 0, null);
                    energymeters.Add(tempMeter);
                    _ = db.CreateEnergyMeter(tempMeter);
                }               
                foreach (var item in EnergyMeters)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Meter", item.latitude, item.longitude, 0, null);
                    energymeters.Add(tempMeter);
                    _ = db.CreateEnergyMeter(tempMeter);
                }

                OccupancyMeters = occupancyManager.LoadOccupancyMeterList(Building_name);
                SolarMeters = solarManager.LoadSolarMeterList(Building_name);

                var building = new BuildingModel(Building_name, Latitude, Longitude, energymeters, OccupancyMeters, SolarMeters);
                _ = db.CreateBuilding(building);
                InitialPopulateDataBaseAsync();    
                foreach(var item in EnergyMeters)
                {
                    item.data.Clear();
                }
                RunBuildingDT();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Building_name} building did not initialise + {e}");
            }
        }
        private void InitialPopulateDataBaseAsync()
        {
            InitialiseEnergyMeterData();
            InitialContextGeneration(startingDate, apiCaller.GetCurrentDateTime().Item1);
            _ = SaveToDataBaseInitialAsync();
        }
        private void InitialiseEnergyMeterData()
        {
            foreach (var item in EnergyMeters)
            {
                item.data = energyManager.GetMeterData(startingDate, apiCaller.GetCurrentDateTime().Item1, item.meterid);
                if (item.data == null)
                {
                    EnergyMeters.Remove(item);
                    Console.WriteLine($"{item.meterid} energy meter removed from {Building_name}");
                }
                else if (item.data.Count > 0)
                {
                    item.latest_power = item.data[item.data.Count - 1].ptot_kw;
                    item.latest_timestamp = item.data[item.data.Count - 1].timestamp;
                    latestBuildingEnergy += item.latest_power;
                }
            }
            if(EnergyMeters.Count > 1)
            {
                buildingEnergyMeter.data = energyManager.GetMeterData(startingDate, apiCaller.GetCurrentDateTime().Item1, buildingEnergyMeter.meterid);
                if (buildingEnergyMeter.data.Count > 0)
                {
                    buildingEnergyMeter.latest_power = buildingEnergyMeter.data[buildingEnergyMeter.data.Count - 1].ptot_kw;
                    buildingEnergyMeter.latest_timestamp = buildingEnergyMeter.data[buildingEnergyMeter.data.Count - 1].timestamp;
                    mainLatestBuildingEnergy += buildingEnergyMeter.latest_power;
                }
            }
        }
        private void InitialContextGeneration(String startDate, String endDate)
        {
            CalculateInitialEnergyDayAverage(startDate, endDate);
            CalculateInitialEnergyMonthAverage(startDate, endDate);
            CalculateInitialEnergyYearAverage(startDate, endDate);
        }
        
        // Energy Data
        public void CalculateInitialEnergyDayAverage(string startDate, string endDate)
        {
            foreach (var item in EnergyMeters)
            {
                var temp = CalculateAverages(item, startDate, endDate, "Day");
                item.day_average = temp.day_average;
                item.day_max = temp.day_max;
                item.day_tot = temp.day_tot;
            }
            if (EnergyMeters.Count > 1)
            {
                var temp = CalculateAverages(buildingEnergyMeter, startDate, endDate, "Day");
                buildingEnergyMeter.day_average = temp.day_average;
                buildingEnergyMeter.day_max = temp.day_max;
                buildingEnergyMeter.day_tot = temp.day_tot;
            }
        }
        public void CalculateInitialEnergyMonthAverage(string startDate, string endDate)
        {
            foreach (var item in EnergyMeters)
            {
                var temp = CalculateAverages(item, startDate, endDate, "Month");
                item.month_average = temp.month_average;
                item.month_max = temp.month_max;
                item.month_tot = temp.month_tot;
            }
            if (EnergyMeters.Count > 1)
            {
                var temp = CalculateAverages(buildingEnergyMeter, startDate, endDate, "Month");
                buildingEnergyMeter.month_average = temp.month_average;
                buildingEnergyMeter.month_max = temp.month_max;
                buildingEnergyMeter.month_tot = temp.month_tot;
            }
        }
        public void CalculateInitialEnergyYearAverage(string startDate, string endDate)
        {
            foreach (var item in EnergyMeters)
            {
                var temp = CalculateAverages(item, startDate, endDate, "Year");
                item.year_average = temp.year_average;
                item.year_max = temp.year_max;
                item.year_tot = temp.year_tot;
            }
            if (EnergyMeters.Count > 1)
            {
                var temp = CalculateAverages(buildingEnergyMeter, startDate, endDate, "Year");
                buildingEnergyMeter.year_average = temp.year_average;
                buildingEnergyMeter.year_max = temp.year_max;
                buildingEnergyMeter.year_tot = temp.year_tot;
            }
        }
        private EnergyMeterData CalculateAverages(EnergyMeterData energyMeter, string startDate, string endDate, string type)
        {
            if (type == "Day")
            {
                energyMeter.day_average = new List<EnergyAverageData>();
                energyMeter.day_max = new List<EnergyAverageData>();
                energyMeter.day_tot = new List<EnergyAverageData>();
                int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
                int startDay, startMonth, startYear;
                (startYear, startMonth, startDay) = apiCaller.GetDate(startDate);
                int endDay, endMonth, endYear;
                (endYear, endMonth, endDay) = apiCaller.GetDate(endDate);
                double tempEnergy = 0;
                double maxEnergy = 0;
                int count = 0;

                EnergyAverageData tempData = new EnergyAverageData();

                for (int i = 0; i < energyMeter.data.Count; i++)
                {
                    (year, month, day) = apiCaller.GetDate(energyMeter.data[i].timestamp);

                    if (((year != prevYear) || (month != prevMonth) || (day != prevDay)) && ((year >= startYear) || (month >= startMonth) || (day >= startDay))
                        && ((year <= endYear) || (month <= endMonth) || (day <= endDay)))
                    {
                        foreach (var data in energyMeter.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = apiCaller.GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                            {
                                if(data.difference_imp_kwh > maxEnergy)
                                {
                                    maxEnergy = data.ptot_kw;
                                }
                                tempEnergy += data.difference_imp_kwh;
                                count++;
                            }
                        }

                        tempData = new EnergyAverageData();
                        tempData.timestamp = year + "-" + month + "-" + day;
                        tempData.ptot_kw = tempEnergy / (count/12);
                        energyMeter.day_average.Add(tempData);
                        var temp = new EnergyAverageData();
                        temp.timestamp = year + "-" + month + "-" + day;
                        temp.ptot_kw = maxEnergy;
                        energyMeter.day_max.Add(temp);
                        var tempTot = new EnergyAverageData();
                        tempTot.timestamp = year + "-" + month + "-" + day;
                        tempTot.ptot_kw = tempEnergy;
                        energyMeter.day_tot.Add(tempTot);
                        tempEnergy = 0;
                        maxEnergy = 0;
                        prevYear = year;
                        prevMonth = month;
                        prevDay = day;
                    }
                }
            }
            else if (type == "Month")
            {
                energyMeter.month_average = new List<EnergyAverageData>();
                energyMeter.month_max = new List<EnergyAverageData>();
                energyMeter.month_tot = new List<EnergyAverageData>();
                int day, month, year, prevMonth = 0, prevYear = 0;
                int startDay, startMonth, startYear;
                (startYear, startMonth, startDay) = apiCaller.GetDate(startDate);
                int endDay, endMonth, endYear;
                (endYear, endMonth, endDay) = apiCaller.GetDate(endDate);
                double tempEnergy = 0;
                double maxEnergy = 0;
                int count = 0;
                EnergyAverageData tempData = new EnergyAverageData();

                for (int i = 0; i < energyMeter.data.Count; i++)
                {
                    (year, month, day) = apiCaller.GetDate(energyMeter.data[i].timestamp);

                    if (((year != prevYear) || (month != prevMonth)) && ((year >= startYear) || (month >= startMonth))
                        && ((year <= endYear) || (month <= endMonth)))
                    {
                        foreach (var data in energyMeter.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = apiCaller.GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth))
                            {
                                if (data.ptot_kw > maxEnergy)
                                {
                                    maxEnergy = data.ptot_kw;
                                }
                                tempEnergy += data.difference_imp_kwh;
                                count++;
                            }
                        }

                        tempData = new EnergyAverageData();
                        tempData.timestamp = year + "-" + month;
                        tempData.ptot_kw = tempEnergy / (count/12);
                        energyMeter.month_average.Add(tempData);
                        var temp = new EnergyAverageData();
                        temp.timestamp = year + "-" + month;
                        temp.ptot_kw = maxEnergy;
                        energyMeter.month_max.Add(temp);
                        var tempTot = new EnergyAverageData();
                        tempTot.timestamp = year + "-" + month;
                        tempTot.ptot_kw = tempEnergy;
                        energyMeter.month_tot.Add(tempTot);
                        tempEnergy = 0;
                        maxEnergy = 0;
                        prevYear = year;
                        prevMonth = month;
                    }
                }
            }
            else if (type == "Year")
            {
                energyMeter.year_average = new List<EnergyAverageData>();
                energyMeter.year_max = new List<EnergyAverageData>();
                energyMeter.year_tot = new List<EnergyAverageData>();
                int day, month, year, prevYear = 0;
                int startDay, startMonth, startYear;
                (startYear, startMonth, startDay) = apiCaller.GetDate(startDate);
                int endDay, endMonth, endYear;
                (endYear, endMonth, endDay) = apiCaller.GetDate(endDate);
                double tempEnergy = 0;
                double maxEnergy = 0;
                int count = 0;
                EnergyAverageData tempData = new EnergyAverageData();

                for (int i = 0; i < energyMeter.data.Count; i++)
                {
                    (year, month, day) = apiCaller.GetDate(energyMeter.data[i].timestamp);

                    if (((year != prevYear)) && ((year >= startYear))
                        && ((year <= endYear)))
                    {
                        foreach (var data in energyMeter.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = apiCaller.GetDate(data.timestamp);
                            if ((year == tempYear))
                            {
                                if (data.ptot_kw > maxEnergy)
                                {
                                    maxEnergy = data.ptot_kw;
                                }
                                tempEnergy += data.difference_imp_kwh;
                                count++;
                            }
                        }

                        tempData = new EnergyAverageData();
                        tempData.timestamp = year.ToString();
                        tempData.ptot_kw = tempEnergy / (count/12);
                        energyMeter.year_average.Add(tempData);
                        var temp = new EnergyAverageData();
                        temp.timestamp = year.ToString();
                        temp.ptot_kw = maxEnergy;
                        energyMeter.year_max.Add(temp);
                        var tempTot = new EnergyAverageData();
                        tempTot.timestamp = year.ToString();
                        tempTot.ptot_kw = tempEnergy;
                        energyMeter.year_tot.Add(tempTot);
                        tempEnergy = 0;
                        maxEnergy = 0;
                        prevYear = year;
                    }
                }
            }

            return energyMeter;
        }
        // Occupancy Meters

        // Solar Meters

        // Water Meters

        private async Task SaveToDataBaseInitialAsync()
        {
            // Save Energy meter day averages to database
            foreach (var item in EnergyMeters)
            {
                for (int i = 0; i < item.day_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Day Average", item.latitude, item.longitude, item.day_average[i].ptot_kw, item.day_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.month_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Month Average", item.latitude, item.longitude, item.month_average[i].ptot_kw, item.month_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.year_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Year Average", item.latitude, item.longitude, item.year_average[i].ptot_kw, item.year_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.day_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Day Max", item.latitude, item.longitude, item.day_max[i].ptot_kw, item.day_max[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.month_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Month Max", item.latitude, item.longitude, item.month_max[i].ptot_kw, item.month_max[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.year_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Year Max", item.latitude, item.longitude, item.year_max[i].ptot_kw, item.year_max[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.day_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Day Total", item.latitude, item.longitude, item.day_tot[i].ptot_kw, item.day_tot[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.month_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Month Total", item.latitude, item.longitude, item.month_tot[i].ptot_kw, item.month_tot[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.year_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, "Year Total", item.latitude, item.longitude, item.year_tot[i].ptot_kw, item.year_tot[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
            }
            var tempCurrent = new EnergyMeterModel("Current", 0, "Current", Latitude, Longitude, latestBuildingEnergy, "");
            await db.CreateEnergyMeter(tempCurrent);
            if (EnergyMeters.Count > 1)
            {
                for (int i = 0; i < buildingEnergyMeter.day_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Day Average", buildingEnergyMeter.latitude, 
                        buildingEnergyMeter.longitude, buildingEnergyMeter.day_average[i].ptot_kw, buildingEnergyMeter.day_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.month_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Month Average", buildingEnergyMeter.latitude,
                        buildingEnergyMeter.longitude, buildingEnergyMeter.month_average[i].ptot_kw, buildingEnergyMeter.month_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.year_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Year Average", buildingEnergyMeter.latitude,
                        buildingEnergyMeter.longitude, buildingEnergyMeter.year_average[i].ptot_kw, buildingEnergyMeter.year_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.day_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Day Max",
                        buildingEnergyMeter.latitude, buildingEnergyMeter.longitude, buildingEnergyMeter.day_max[i].ptot_kw, buildingEnergyMeter.day_max[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.month_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Month Max", 
                        buildingEnergyMeter.latitude, buildingEnergyMeter.longitude, buildingEnergyMeter.month_max[i].ptot_kw, buildingEnergyMeter.month_max[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.year_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Year Max", 
                        buildingEnergyMeter.latitude, buildingEnergyMeter.longitude, buildingEnergyMeter.year_max[i].ptot_kw, buildingEnergyMeter.year_max[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.day_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Day Total", 
                        buildingEnergyMeter.latitude, buildingEnergyMeter.longitude, buildingEnergyMeter.day_tot[i].ptot_kw, buildingEnergyMeter.day_tot[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.month_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Month Total",
                        buildingEnergyMeter.latitude, buildingEnergyMeter.longitude, buildingEnergyMeter.month_tot[i].ptot_kw, buildingEnergyMeter.month_tot[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < buildingEnergyMeter.year_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(buildingEnergyMeter.description, buildingEnergyMeter.meterid, "Year Total",
                        buildingEnergyMeter.latitude, buildingEnergyMeter.longitude, buildingEnergyMeter.year_tot[i].ptot_kw, buildingEnergyMeter.year_tot[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                tempCurrent = new EnergyMeterModel("MainMeterCurrent", 0, "Main Current", Latitude, Longitude, mainLatestBuildingEnergy, "");
                await db.CreateEnergyMeter(tempCurrent);
            }
            Console.WriteLine($"{Building_name} DT has been initialised");
            Initialised = true;
        }

        #endregion

        #region DT Running Functions
        public void RunBuildingDT()
        {
            //stopWatch.Start();
            while (true)
            {
                //double ts = stopWatch.Elapsed.TotalSeconds;
                //if (ts >= 5)
                //{
                    Thread.Sleep(60000);
                    _ = GetCurrentMeterDataAsync();
                    //Console.WriteLine("Running " + Building_name);
                    //stopWatch.Restart();
                //}
            }
        }
        private async Task GetCurrentMeterDataAsync()
        {
            await GetCurrentEnergyMeterDataAsync();
        }
        // Energy Meter Data
        /*public async Task<string> AccessDatabaseAsync(int meterid, string date)
        {
            var temp = await db.GetEnergyMeterReading(meterid, date);
            var info = JsonConvert.SerializeObject(temp[0]);
            return info;
        }*/
        private async Task GetCurrentEnergyMeterDataAsync()
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
                        var data = tempData[tempData.Count - 1].difference_imp_kwh;
                        /*var temp = await db.GetEnergyMeterReading(item.meterid, utilities.DecodeTimestamp(item.previous_timestamp, "Day"));
                        if (temp != null)
                        {
                            Console.WriteLine($"Old day value for energy for {item.description} {item.previous_timestamp} - {temp[0].Power_Tot}");
                        }*/
                        var newDayEnergyData = await CalculateEnergyNewAverageAsync(item.meterid, data, item.previous_timestamp, "Day");
                        var newMonthEnergyData = await CalculateEnergyNewAverageAsync(item.meterid, data, item.previous_timestamp, "Month");
                        var newYearEnergyData = await CalculateEnergyNewAverageAsync(item.meterid, data, item.previous_timestamp, "Year");
                        var newDayMaxEnergy = await CalculateEnergyNewMaxAsync(item.meterid, data, item.previous_timestamp, "Day Max");
                        var newMonthMaxEnergy = await CalculateEnergyNewMaxAsync(item.meterid, data, item.previous_timestamp, "Month Max");
                        var newYearMaxEnergy = await CalculateEnergyNewMaxAsync(item.meterid, data, item.previous_timestamp, "Year Max");
                        var newDayTotalEnergy = await CalculateEnergyNewTotalAsync(item.meterid, data, item.previous_timestamp, "Day Total");
                        var newMonthTotalEnergy = await CalculateEnergyNewTotalAsync(item.meterid, data, item.previous_timestamp, "Month Total");
                        var newYearTotalEnergy = await CalculateEnergyNewTotalAsync(item.meterid, data, item.previous_timestamp, "Year Total");
                        if (newDayEnergyData.Timestamp != null)
                        {
                            await db.UpdateEnergyMeter(newDayEnergyData);
                            await db.UpdateEnergyMeter(newMonthEnergyData);
                            await db.UpdateEnergyMeter(newYearEnergyData);
                            await db.UpdateEnergyMeter(newDayMaxEnergy);
                            await db.UpdateEnergyMeter(newMonthMaxEnergy);
                            await db.UpdateEnergyMeter(newYearMaxEnergy);
                            await db.UpdateEnergyMeter(newDayTotalEnergy);
                            await db.UpdateEnergyMeter(newMonthTotalEnergy);
                            await db.UpdateEnergyMeter(newYearTotalEnergy);
                        }
                        else if (newDayEnergyData.Timestamp == null)
                        {
                            var tempDayMeter = new EnergyMeterModel(item.description, item.meterid, "Day Average", item.latitude, item.longitude, data, utilities.DecodeTimestamp(item.latest_timestamp, "Day"));
                            await db.CreateEnergyMeter(tempDayMeter);
                            tempDayMeter = new EnergyMeterModel(item.description, item.meterid, "Day Max", item.latitude, item.longitude, item.latest_power, utilities.DecodeTimestamp(item.latest_timestamp, "Day"));
                            await db.CreateEnergyMeter(tempDayMeter);
                            tempDayMeter = new EnergyMeterModel(item.description, item.meterid, "Day Total", item.latitude, item.longitude, data, utilities.DecodeTimestamp(item.latest_timestamp, "Day"));
                            await db.CreateEnergyMeter(tempDayMeter);

                            var prevMonth = utilities.DecodeTimestamp(newDayEnergyData.Timestamp, "Month");
                            var prevYear = utilities.DecodeTimestamp(newDayEnergyData.Timestamp, "Year");

                            if (utilities.DecodeTimestamp(item.latest_timestamp, "Month") == prevMonth)
                            {
                                await db.UpdateEnergyMeter(newMonthEnergyData);
                            }
                            else if (utilities.DecodeTimestamp(item.latest_timestamp, "Month") != prevMonth)
                            {
                                var tempMonthMeter = new EnergyMeterModel(item.description, item.meterid, "Month Average", item.latitude, item.longitude, data, utilities.DecodeTimestamp(item.latest_timestamp, "Month"));
                                await db.CreateEnergyMeter(tempMonthMeter);
                                tempMonthMeter = new EnergyMeterModel(item.description, item.meterid, "Month Max", item.latitude, item.longitude, item.latest_power, utilities.DecodeTimestamp(item.latest_timestamp, "Month"));
                                await db.CreateEnergyMeter(tempMonthMeter);
                                tempMonthMeter = new EnergyMeterModel(item.description, item.meterid, "Month Total", item.latitude, item.longitude, data, utilities.DecodeTimestamp(item.latest_timestamp, "Month"));
                                await db.CreateEnergyMeter(tempMonthMeter);
                            }

                            if (utilities.DecodeTimestamp(item.latest_timestamp, "Year") == prevYear)
                            {
                                await db.UpdateEnergyMeter(newYearEnergyData);
                            }
                            else if (utilities.DecodeTimestamp(item.latest_timestamp, "Year") != prevYear)
                            {
                                var tempYearMeter = new EnergyMeterModel(item.description, item.meterid, "Year Average", item.latitude, item.longitude, data, utilities.DecodeTimestamp(item.latest_timestamp, "Year"));
                                await db.CreateEnergyMeter(tempYearMeter);
                                tempYearMeter = new EnergyMeterModel(item.description, item.meterid, "Year Max", item.latitude, item.longitude, item.latest_power, utilities.DecodeTimestamp(item.latest_timestamp, "Year"));
                                await db.CreateEnergyMeter(tempYearMeter);
                                tempYearMeter = new EnergyMeterModel(item.description, item.meterid, "Year Total", item.latitude, item.longitude, data, utilities.DecodeTimestamp(item.latest_timestamp, "Year"));
                                await db.CreateEnergyMeter(tempYearMeter);
                            }
                        }
                        item.NewDataAvailable = true;
                        //temp = await db.GetEnergyMeterReading(item.meterid, utilities.DecodeTimestamp(item.latest_timestamp, "Day"));
                        //Console.WriteLine($"Updated day value for energy for {item.description} {item.latest_timestamp} - {temp[0].Power_Tot}");
                        //Console.WriteLine(Building_name + " Updated");
                    }
                }
            }
            UpdateDataAvailable();
        }
        public void UpdateDataAvailable()
        {
            bool initial = true;
            foreach (var item in EnergyMeters)
            {
                if (item.NewDataAvailable == false)
                {
                    initial = false;
                }
            }
            if (initial)
            {
                NewEnergyDataAvailable = true;
                _ = UpdateLatestEnergyDataAsync();
            }
        }
        public void ResetDataAvailable()
        {
            NewEnergyDataAvailable = false;
            foreach (var item in EnergyMeters)
            {
                item.NewDataAvailable = false;
            }
        }
        private async Task UpdateLatestEnergyDataAsync()
        {
            // Update latest energy reading
            latestBuildingEnergy = 0;
            foreach (var item in EnergyMeters)
            {
                latestBuildingEnergy += item.latest_power;
            }
            var temp = await db.GetLatestEnergyReading();
            temp[0].Power_Tot = latestBuildingEnergy;
            await db.UpdateEnergyMeter(temp[0]);
        }
        #endregion

        #region Services

        // Energy Services
        public List<string> ReturnLocation()
        {
            List<string> location = new List<string>();
            location.Add(Latitude);
            location.Add(Longitude);
            return location;
        }
        public async Task<double> ReturnLatestBuildingEnergyAsync()
        {
            try
            {
                var temp = await db.GetLatestEnergyReading();
                return (double)temp[0].Power_Tot;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public async Task<List<EnergyMeterModel>> ReturnBuildingEnergyAveragesAsync(List<string> dateList, string type)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetBuildingEnergyReading(Building_name, date, type);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        public async Task<List<EnergyMeterModel>> ReturnBuildingEnergyDataAsync(List<string> dateList, string type)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetBuildingEnergyReading(Building_name, date, type);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        private async Task<List<EnergyMeterModel>> ReturnEnergyAveragesAsync(int meterID, List<string> dateList, string type)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetEnergyMeterReading(meterID, date, type);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        private async Task<List<EnergyMeterModel>> ReturnEnergyDataAsync(int meterID, List<string> dateList, string type)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetEnergyMeterReading(meterID, date, type);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        public async Task<EnergyMeterModel> CalculateEnergyNewAverageAsync(int meterid, double newPower, string prevTime, string type)
        {
            string prevTimestamp = utilities.DecodeTimestamp(prevTime, type);
            int currentNumberofTimestamps = 0;
            if (type == "Day")
            {
                currentNumberofTimestamps = (DateTime.Now.Hour * 12) + (DateTime.Now.Minute / 5);
            }
            else if (type == "Month")
            {
                currentNumberofTimestamps = (DateTime.Now.Day * 288) + (DateTime.Now.Hour * 12) + (DateTime.Now.Minute / 5);
            }
            else if (type == "Year")
            {
                currentNumberofTimestamps = (DateTime.Now.Day * 8640) + (DateTime.Now.Day * 288) + (DateTime.Now.Hour * 12) + (DateTime.Now.Minute / 5);
            }

            var temp = await db.GetEnergyMeterReading(meterid, prevTimestamp, $"{type} Average");
            EnergyMeterModel result = null;
            if (temp.Count != 0)
            {
                double newp = (double)((temp[0].Power_Tot * (currentNumberofTimestamps)) + newPower) / (currentNumberofTimestamps + 1);
                temp[0].Power_Tot = newp;
                result = temp[0];
            }
            else if (temp.Count == 0)
            {
                result = new EnergyMeterModel(null, 0, null, null, null, 0, null);
            }

            return result;
        }
        private async Task<EnergyMeterModel> CalculateEnergyNewMaxAsync(int meterid, double newPower, string prevTime, string type)
        {
            string prevTimestamp = utilities.DecodeTimestamp(prevTime, type);

            var temp = await db.GetEnergyMeterReading(meterid, prevTimestamp, type);
            EnergyMeterModel result = null;
            if (temp.Count != 0)
            {
                if (newPower > temp[0].Power_Tot)
                {
                    temp[0].Power_Tot = newPower;
                    result = temp[0];
                }
            }
            else if (temp.Count == 0)
            {
                result = new EnergyMeterModel(null, 0, null, null, null, 0, null);
            }

            return result;
        }
        private async Task<EnergyMeterModel> CalculateEnergyNewTotalAsync(int meterid, double newPower, string prevTime, string type)
        {
            string prevTimestamp = utilities.DecodeTimestamp(prevTime, type);

            var temp = await db.GetEnergyMeterReading(meterid, prevTimestamp, type);
            EnergyMeterModel result = null;
            if (temp.Count != 0)
            {
                    temp[0].Power_Tot += newPower;
                    result = temp[0];
            }
            else if (temp.Count == 0)
            {
                result = new EnergyMeterModel(null, 0, null, null, null, 0, null);
            }

            return result;
        }
        public async Task<double> GetTotalEnergyAsync(string date, string type)
        {
            try
            {
                List<string> dates = new List<string>();
                dates.Add(date);
                double totPower = 0;

                if (EnergyMeters.Count > 1)
                {
                    foreach (var item in EnergyMeters)
                    {
                        if (item.meterid != buildingEnergyMeter.meterid)
                        {
                            var temp = await ReturnEnergyAveragesAsync(item.meterid, dates, type);
                            totPower += (double)temp[0].Power_Tot;
                        }
                    }
                }
                else
                {
                    var temp = await ReturnEnergyAveragesAsync(EnergyMeters[0].meterid, dates, type);
                    totPower = (double)temp[0].Power_Tot;
                }

                return totPower;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Building_name} failed to initialise for {date} for {type}");
            };
            return 0;
        }
        public async Task<double> GetTotalEnergyDataAsync(string date, string type)
        {
            try
            {
                List<string> dates = new List<string>();
                dates.Add(date);
                double totPower = 0;

                if (EnergyMeters.Count > 1)
                {
                    foreach (var item in EnergyMeters)
                    {
                        if (item.meterid != buildingEnergyMeter.meterid)
                        {
                            var temp = await ReturnEnergyDataAsync(item.meterid, dates, type);
                            totPower += (double)temp[0].Power_Tot;
                        }
                    }
                }
                else
                {
                    var temp = await ReturnEnergyDataAsync(EnergyMeters[0].meterid, dates, type);
                    totPower = (double)temp[0].Power_Tot;
                }

                return totPower;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Building_name} failed to initialise for {date} for {type}");
            };
            return 0;
        }
        private List<InformationModel> GenerateInformationList(List<EnergyMeterModel> energyList)
        {
            List<InformationModel> infoModelList = new List<InformationModel>();
            foreach (var item in energyList)
            {
                string dt_Type = "Building";
                InformationModel temp = new InformationModel {
                    DataType = "Energy", DT_Type = dt_Type, DT_name = item.EnergyMeter_name, Longitude = item.Longitude, Latitude = item.Latitude
            , Value = (double)item.Power_Tot, Timestamp = item.Timestamp
                };
                infoModelList.Add(temp);
            }
            return infoModelList;
        }
        public async Task<string> ServiceHandlerAsync(MessageModel message)
        {
            if (message.DataType == "Energy")
            {
                if (message.MessageType == "CurrentData")
                {
                    var tempEnergy = await ReturnLatestBuildingEnergyAsync();
                    EnergyMeterModel temp = new EnergyMeterModel(Building_name, 0, "Current", Latitude, Longitude, tempEnergy, "Latest Reading");
                    List<EnergyMeterModel> tempEnergyList = new List<EnergyMeterModel>();
                    tempEnergyList.Add(temp);
                    List<InformationModel> tempList = GenerateInformationList(tempEnergyList);
                    var tempMess = JsonConvert.SerializeObject(tempList);
                    return tempMess;
                }
                else if (message.MessageType == "Averages")
                {
                    message.startDate = utilities.ChangeDateFormat(message.startDate);
                    message.endDate = utilities.ChangeDateFormat(message.endDate);
                    var temp = await ReturnBuildingEnergyAveragesAsync(utilities.GenerateDateList(message.startDate, message.endDate, message.timePeriod), $"{message.timePeriod} Average");
                    var infoModelList = GenerateInformationList(temp);
                    var response = JsonConvert.SerializeObject(infoModelList);
                    return response;
                }
                else if (message.MessageType == "Max")
                {
                    message.startDate = utilities.ChangeDateFormat(message.startDate);
                    message.endDate = utilities.ChangeDateFormat(message.endDate);
                    var temp = await ReturnBuildingEnergyDataAsync(utilities.GenerateDateList(message.startDate, message.endDate, message.timePeriod), $"{message.timePeriod} Max");
                    var infoModelList = GenerateInformationList(temp);
                    var response = JsonConvert.SerializeObject(infoModelList);
                    return response;
                }
                else if (message.MessageType == "Total")
                {
                    message.startDate = utilities.ChangeDateFormat(message.startDate);
                    message.endDate = utilities.ChangeDateFormat(message.endDate);
                    var temp = await ReturnBuildingEnergyDataAsync(utilities.GenerateDateList(message.startDate, message.endDate, message.timePeriod), $"{message.timePeriod} Total");
                    var infoModelList = GenerateInformationList(temp);
                    var response = JsonConvert.SerializeObject(infoModelList);
                    return response;
                }
            }
            else if (message.DataType == "DigitalTwins")
            {
                if (message.MessageType == "ChildDTList")
                {
                    ChildDTModel temp = new ChildDTModel("None", "None");
                    var tempList = new List<ChildDTModel>();
                    tempList.Add(temp);
                    var tempMess = JsonConvert.SerializeObject(tempList);
                    return tempMess;
                }
            }
            else if (message.DataType == "Initialisation")
            {
                if (message.MessageType == "Status")
                {
                    return Initialised.ToString();
                }
                else if (message.MessageType == "LatestEnergy")
                {
                    var energy = await ReturnLatestBuildingEnergyAsync();
                    return energy.ToString();
                }
                else if (message.MessageType == "Averages")
                {
                    var energy = await GetTotalEnergyAsync(message.startDate, $"{message.timePeriod} Average");
                    return energy.ToString();
                }
                else if (message.MessageType == "Max")
                {
                    var energy = await GetTotalEnergyDataAsync(message.startDate, $"{message.timePeriod} Max");
                    return energy.ToString();
                }
                else if (message.MessageType == "Total")
                {
                    var energy = await GetTotalEnergyDataAsync(message.startDate, $"{message.timePeriod} Total");
                    return energy.ToString();
                }
            }
            else if (message.DataType == "Operations")
            {
                if(message.MessageType == "LatestTimeStamp")
                {
                    return EnergyMeters[0].latest_timestamp;
                }
                else if (message.MessageType == "LatestEnergy")
                {
                    var energy = await ReturnLatestBuildingEnergyAsync();
                    return energy.ToString();
                }
                else if (message.MessageType == "NewEnergyDataStatus")
                {
                    return NewEnergyDataAvailable.ToString();
                }
                else if (message.MessageType == "ResetNewDataAvailable")
                {
                    ResetDataAvailable();
                    return "Complete";
                }
            }
                    return "";
        }
        #endregion

    }
}

