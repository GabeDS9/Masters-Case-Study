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

                foreach (var item in EnergyMeters)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, item.latitude, item.longitude, 0, null);
                    energymeters.Add(tempMeter);
                    _ = db.CreateEnergyMeter(tempMeter);
                }

                OccupancyMeters = occupancyManager.LoadOccupancyMeterList(Building_name);
                SolarMeters = solarManager.LoadSolarMeterList(Building_name);

                var building = new BuildingModel(Building_name, Latitude, Longitude, energymeters, OccupancyMeters, SolarMeters);
                _ = db.CreateBuilding(building);
                _ = InitialPopulateDataBaseAsync();
                RunBuildingDT();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Building_name} building did not initialise + {e}");
            }
        }
        private async Task InitialPopulateDataBaseAsync()
        {
            InitialiseEnergyMeterData();
            InitialContextGeneration(startingDate, apiCaller.GetCurrentDateTime().Item1);
            await SaveToDataBaseInitialAsync();
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
                item.day_average = new List<EnergyAverageData>();

                int day, month, year, prevDay = 0, prevMonth = 0, prevYear = 0;
                int startDay, startMonth, startYear;
                (startYear, startMonth, startDay) = apiCaller.GetDate(startDate);
                int endDay, endMonth, endYear;
                (endYear, endMonth, endDay) = apiCaller.GetDate(endDate);
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
        public void CalculateInitialEnergyMonthAverage(string startDate, string endDate)
        {
            foreach (var item in EnergyMeters)
            {
                item.month_average = new List<EnergyAverageData>();

                int day, month, year, prevMonth = 0, prevYear = 0;
                int startDay, startMonth, startYear;
                (startYear, startMonth, startDay) = apiCaller.GetDate(startDate);
                int endDay, endMonth, endYear;
                (endYear, endMonth, endDay) = apiCaller.GetDate(endDate);
                double tempEnergy = 0;
                int count = 0;
                EnergyAverageData tempData = new EnergyAverageData();

                for (int i = 0; i < item.data.Count; i++)
                {
                    (year, month, day) = apiCaller.GetDate(item.data[i].timestamp);

                    if (((year != prevYear) || (month != prevMonth)) && ((year >= startYear) || (month >= startMonth))
                        && ((year <= endYear) || (month <= endMonth)))
                    {
                        foreach (var data in item.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = apiCaller.GetDate(data.timestamp);
                            if ((year == tempYear) && (month == tempMonth))
                            {
                                tempEnergy += data.ptot_kw;
                                count++;
                            }
                        }

                        tempData = new EnergyAverageData();
                        tempData.timestamp = year + "-" + month;
                        tempData.ptot_kw = tempEnergy / count;
                        item.month_average.Add(tempData);
                        prevYear = year;
                        prevMonth = month;
                    }
                }
            }
        }
        public void CalculateInitialEnergyYearAverage(string startDate, string endDate)
        {
            foreach (var item in EnergyMeters)
            {
                item.year_average = new List<EnergyAverageData>();

                int day, month, year, prevYear = 0;
                int startDay, startMonth, startYear;
                (startYear, startMonth, startDay) = apiCaller.GetDate(startDate);
                int endDay, endMonth, endYear;
                (endYear, endMonth, endDay) = apiCaller.GetDate(endDate);
                double tempEnergy = 0;
                int count = 0;
                EnergyAverageData tempData = new EnergyAverageData();

                for (int i = 0; i < item.data.Count; i++)
                {
                    (year, month, day) = apiCaller.GetDate(item.data[i].timestamp);

                    if (((year != prevYear)) && ((year >= startYear))
                        && ((year <= endYear)))
                    {
                        foreach (var data in item.data)
                        {
                            int tempDay, tempMonth, tempYear;
                            (tempYear, tempMonth, tempDay) = apiCaller.GetDate(data.timestamp);
                            if ((year == tempYear))
                            {
                                tempEnergy += data.ptot_kw;
                                count++;
                            }
                        }

                        tempData = new EnergyAverageData();
                        tempData.timestamp = year.ToString();
                        tempData.ptot_kw = tempEnergy / count;
                        item.year_average.Add(tempData);
                        prevYear = year;
                    }
                }
            }
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
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, item.latitude, item.longitude, item.day_average[i].ptot_kw, item.day_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.month_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, item.latitude, item.longitude, item.month_average[i].ptot_kw, item.month_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
                for (int i = 0; i < item.year_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(item.description, item.meterid, item.latitude, item.longitude, item.year_average[i].ptot_kw, item.year_average[i].timestamp);
                    await db.CreateEnergyMeter(tempMeter);
                }
            }
            var tempCurrent = new EnergyMeterModel("Current", 0, Latitude, Longitude, latestBuildingEnergy, "");
            await db.CreateEnergyMeter(tempCurrent);
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
        public async Task<string> AccessDatabaseAsync(int meterid, string date)
        {
            var temp = await db.GetEnergyMeterReading(meterid, date);
            var info = JsonConvert.SerializeObject(temp[0]);
            return info;
        }
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
                        /*var temp = await db.GetEnergyMeterReading(item.meterid, utilities.DecodeTimestamp(item.previous_timestamp, "Day"));
                        if (temp != null)
                        {
                            Console.WriteLine($"Old day value for energy for {item.description} {item.previous_timestamp} - {temp[0].Power_Tot}");
                        }*/
                        var newDayEnergyData = await CalculateEnergyNewAverageAsync(item.meterid, item.latest_power, item.previous_timestamp, "Day");
                        var newMonthEnergyData = await CalculateEnergyNewAverageAsync(item.meterid, item.latest_power, item.previous_timestamp, "Month");
                        var newYearEnergyData = await CalculateEnergyNewAverageAsync(item.meterid, item.latest_power, item.previous_timestamp, "Year");
                        if (newDayEnergyData.Timestamp != null)
                        {
                            await db.UpdateEnergyMeter(newDayEnergyData);
                            await db.UpdateEnergyMeter(newMonthEnergyData);
                            await db.UpdateEnergyMeter(newYearEnergyData);
                        }
                        else if (newDayEnergyData.Timestamp == null)
                        {
                            var tempDayMeter = new EnergyMeterModel(item.description, item.meterid, item.latitude, item.longitude, item.latest_power, utilities.DecodeTimestamp(item.latest_timestamp, "Day"));
                            await db.CreateEnergyMeter(tempDayMeter);

                            var prevMonth = utilities.DecodeTimestamp(newDayEnergyData.Timestamp, "Month");
                            var prevYear = utilities.DecodeTimestamp(newDayEnergyData.Timestamp, "Year");

                            if (utilities.DecodeTimestamp(item.latest_timestamp, "Month") == prevMonth)
                            {
                                await db.UpdateEnergyMeter(newMonthEnergyData);
                            }
                            else if (utilities.DecodeTimestamp(item.latest_timestamp, "Month") != prevMonth)
                            {
                                var tempMonthMeter = new EnergyMeterModel(item.description, item.meterid, item.latitude, item.longitude, item.latest_power, utilities.DecodeTimestamp(item.latest_timestamp, "Month"));
                                await db.CreateEnergyMeter(tempMonthMeter);
                            }

                            if (utilities.DecodeTimestamp(item.latest_timestamp, "Year") == prevYear)
                            {
                                await db.UpdateEnergyMeter(newYearEnergyData);
                            }
                            else if (utilities.DecodeTimestamp(item.latest_timestamp, "Year") != prevYear)
                            {
                                var tempYearMeter = new EnergyMeterModel(item.description, item.meterid, item.latitude, item.longitude, item.latest_power, utilities.DecodeTimestamp(item.latest_timestamp, "Year"));
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
        public async Task<List<EnergyMeterModel>> ReturnBuildingEnergyAveragesAsync(List<string> dateList)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetBuildingEnergyReading(Building_name, date);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        private async Task<List<EnergyMeterModel>> ReturnEnergyAveragesAsync(int meterID, List<string> dateList)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetEnergyMeterReading(meterID, date);
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

            var temp = await db.GetEnergyMeterReading(meterid, prevTimestamp);
            EnergyMeterModel result = null;
            if (temp.Count != 0)
            {
                double newp = (double)((temp[0].Power_Tot * (currentNumberofTimestamps)) + newPower) / (currentNumberofTimestamps + 1);
                temp[0].Power_Tot = newp;
                result = temp[0];
            }
            else if (temp.Count == 0)
            {
                result = new EnergyMeterModel(null, 0, null, null, 0, null);
            }

            return result;
        }
        public async Task<double> GetTotalEnergyAsync(string date)
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
                        if (item.meterid != EnergyMeters[0].meterid)
                        {
                            var temp = await ReturnEnergyAveragesAsync(item.meterid, dates);
                            totPower += (double)temp[0].Power_Tot;
                        }
                    }
                }
                else
                {
                    var temp = await ReturnEnergyAveragesAsync(EnergyMeters[0].meterid, dates);
                    totPower = (double)temp[0].Power_Tot;
                }

                return totPower;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Building_name} failed to initialise for {date}");
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
                    EnergyMeterModel temp = new EnergyMeterModel(Building_name, 0, Latitude, Longitude, tempEnergy, "Latest Reading");
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
                    var temp = await ReturnBuildingEnergyAveragesAsync(utilities.GenerateDateList(message.startDate, message.endDate, message.timePeriod));
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
                    var energy = await GetTotalEnergyAsync(message.startDate);
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

