using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;
using Utils;
using Models;
using Newtonsoft.Json;

namespace Precinct_DT
{
    public class Precinct
    {
        public string Precinct_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int Port { get; set; }
        public List<Energy_Reticulation_DT.EnergyReticulation> EnergyMeters { get; set; }
        public List<Solar_Reticulation_DT.SolarReticulation> SolarMeters { get; set; }
        public List<Building_DT.Building> Buildings { get; set; }

        private Energy_Reticulation_DT.EnergyReticulationManager energyManager = new Energy_Reticulation_DT.EnergyReticulationManager();
        private Solar_Reticulation_DT.SolarReticulationManager solarManager = new Solar_Reticulation_DT.SolarReticulationManager();
        private Building_DT.BuildingManager buildingManager = new Building_DT.BuildingManager();
        private List<string> buildingNames = new List<string>();

        private APICaller apiCaller = new APICaller();
        private Stopwatch stopWatch = new Stopwatch();
        private PrecinctDBDataAccess db;
        private Utilities utilities = new Utilities();
        private string startingDate;

        private List<EnergyMeterModel> precinctInitialEnergyDayReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyMonthReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyYearReadings = new List<EnergyMeterModel>();
        private bool BuildingsInitialised = false;        
        private string prevEnergyTime;
        private bool EnergyDataAvailable = false;
        private double latestPrecinctEnergy = 0;

        public bool Initialised = false;
        public bool NewEnergyDataAvailable = false;

        Services_Communication.ServerSocket myServer = new Services_Communication.ServerSocket();
        public Precinct(string name, string latitude, string longitude, int port, string iniDate)
        {
            Precinct_name = name;
            Latitude = latitude;
            Longitude = longitude;
            Port = port;
            db = new PrecinctDBDataAccess(Precinct_name.Replace(" ", "_"));
            startingDate = iniDate;
            _ = InitialisePrecinctAsync();
        }

        # region Initialisation Functions
        public async Task InitialisePrecinctAsync()
        {
            Buildings = buildingManager.InitialiseBuildings(Precinct_name, startingDate);
            myServer.SetupServer(Port, null, this, null);
            while (!Initialised)
            {
                CheckInitialisations();
                if (BuildingsInitialised)
                {
                    foreach (var building in Buildings)
                    {
                        buildingNames.Add(building.Building_name);
                        latestPrecinctEnergy += await building.ReturnLatestBuildingEnergyAsync();
                    }

                    var precinct = new PrecinctModel(Precinct_name, Latitude, Longitude, buildingNames);
                    await db.CreatePrecinct(precinct);
                    await InitialPopulateDataBaseAsync();
                    Initialised = true;
                }
            }
            Initialised = true;
            await Task.Run(() => RunPrecinctDTAsync());
        }
        private void CheckInitialisations()
        {
            bool initial = true;
            foreach (var building in Buildings)
            {
                if (building.Initialised == false)
                {
                    initial = false;
                }
            }

            if (initial)
            {
                BuildingsInitialised = true;
            }
        }
        private async Task InitialPopulateDataBaseAsync()
        {
            await InitialContextGenerationAsync(startingDate, apiCaller.GetCurrentDateTime().Item1);
            _ = SaveToDataBaseInitialAsync();
        }
        private async Task InitialContextGenerationAsync(String startDate, String endDate)
        {
            await CalculateInitialEnergyDayAverageAsync(startDate, endDate);
            await CalculateInitialEnergyMonthAverage(startDate, endDate);
            await CalculateInitialEnergyYearAverage(startDate, endDate);
            prevEnergyTime = endDate;
        }
        public async Task CalculateInitialEnergyDayAverageAsync(string startDate, string endDate)
        {
            string type = "Day";
            startDate = utilities.ChangeDateFormat(utilities.DecodeTimestamp(startDate, "Day"));
            endDate = utilities.ChangeDateFormat(utilities.DecodeTimestamp(endDate, "Day"));
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, type);
            double powerTot = 0;
            foreach (var date in dateList)
            {
                foreach (var building in Buildings)
                {
                    powerTot += await building.GetTotalEnergyAsync(date);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, powerTot, date);
                precinctInitialEnergyDayReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
        }
        public async Task CalculateInitialEnergyMonthAverage(string startDate, string endDate)
        {
            string type = "Month";
            startDate = utilities.ChangeDateFormat(utilities.DecodeTimestamp(startDate, "Day"));
            endDate = utilities.ChangeDateFormat(utilities.DecodeTimestamp(endDate, "Day"));
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, type);
            double powerTot = 0;
            foreach (var date in dateList)
            {
                foreach (var building in Buildings)
                {
                    powerTot += await building.GetTotalEnergyAsync(date);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, powerTot, date);
                precinctInitialEnergyMonthReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
        }
        public async Task CalculateInitialEnergyYearAverage(string startDate, string endDate)
        {
            string type = "Year";
            startDate = utilities.ChangeDateFormat(utilities.DecodeTimestamp(startDate, "Day"));
            endDate = utilities.ChangeDateFormat(utilities.DecodeTimestamp(endDate, "Day"));
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, type);
            double powerTot = 0;
            foreach (var date in dateList)
            {
                foreach (var building in Buildings)
                {
                    powerTot += await building.GetTotalEnergyAsync(date);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, powerTot, date);
                precinctInitialEnergyYearReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
        }
        private async Task SaveToDataBaseInitialAsync()
        {
            foreach (var day in precinctInitialEnergyDayReadings)
            {
                await db.CreateEnergyReading(day);
            }
            foreach (var month in precinctInitialEnergyMonthReadings)
            {
                await db.CreateEnergyReading(month);
            }
            foreach (var year in precinctInitialEnergyYearReadings)
            {
                await db.CreateEnergyReading(year);
            }
            var tempCurrent = new EnergyMeterModel("Current", 0, Latitude, Longitude, latestPrecinctEnergy, "");
            await db.CreateEnergyReading(tempCurrent);
            Console.WriteLine($"{Precinct_name} DT has been initialised");
        }
        #endregion

        #region DT Running Functions
        public async Task RunPrecinctDTAsync()
        {
            stopWatch.Start();
            while (true)
            {
                double ts = stopWatch.Elapsed.TotalSeconds;
                if (ts >= 60)
                {
                    await GetUpdatedDataAsync();
                    stopWatch.Restart();
                }
            }
        }
        private async Task GetUpdatedDataAsync()
        {
            await GetUpdatedEnergyDataAsync();
        }
        private async Task GetUpdatedEnergyDataAsync()
        {
            CheckUpdatedData();
            if (EnergyDataAvailable)
            {                        
                double newDayPower = 0;
                string dayDate;                
                foreach (var building in Buildings)
                {
                    dayDate = utilities.DecodeTimestamp(building.EnergyMeters[0].latest_timestamp, "Day");
                    prevEnergyTime = dayDate;
                    newDayPower += await building.GetTotalEnergyAsync(dayDate);
                }
                /*var temp = await db.GetEnergyReading(Precinct_name, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                if (temp != null)
                {
                    Console.WriteLine($"Old day value for energy for {Precinct_name} {prevEnergyTime} - {temp[0].Power_Tot}");
                }*/
                var newDayEnergyData = await CalculateEnergyNewAverageAsync(newDayPower, prevEnergyTime, "Day");
                var newMonthEnergyData = await CalculateEnergyNewAverageAsync(newDayPower, prevEnergyTime, "Month");
                var newYearEnergyData = await CalculateEnergyNewAverageAsync(newDayPower, prevEnergyTime, "Year");

                if (newDayEnergyData.Timestamp != null)
                {
                    await db.UpdateEnergyMeter(newDayEnergyData);
                    await db.UpdateEnergyMeter(newMonthEnergyData);
                    await db.UpdateEnergyMeter(newYearEnergyData);
                }
                else if (newDayEnergyData.Timestamp == null)
                {
                    var tempDayMeter = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, (double)newDayEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                    var tempMonthMeter = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, (double)newMonthEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Month")); 
                    var tempYearMeter = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, (double)newYearEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Year"));
                    await db.CreateEnergyReading(tempDayMeter);
                    await db.CreateEnergyReading(tempMonthMeter);
                    await db.CreateEnergyReading(tempYearMeter);
                }
                ResetUpdatedDataAvailable();
                NewEnergyDataAvailable = true;
                //temp = await db.GetEnergyReading(Precinct_name, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                //Console.WriteLine($"Updated day value for energy for {Precinct_name} {prevEnergyTime} - {temp[0].Power_Tot}");
                //Console.WriteLine(Precinct_name + " Updated");
            }            
        }
        private void CheckUpdatedData()
        {
            bool initial = true;
            foreach (var building in Buildings)
            {
                if (building.NewEnergyDataAvailable == false)
                {
                    initial = false;
                }
            }
            if (initial)
            {
                EnergyDataAvailable = true;
                _ = UpdateLatestEnergyDataAsync();
            }
        }
        private void ResetUpdatedDataAvailable()
        {
            EnergyDataAvailable = false;
            foreach (var building in Buildings)
            {
                building.ResetDataAvailable();
            }
        }
        private async Task UpdateLatestEnergyDataAsync()
        {
            // Update latest energy reading
            latestPrecinctEnergy = 0;
            foreach (var building in Buildings)
            {
                latestPrecinctEnergy += await building.ReturnLatestBuildingEnergyAsync();
            }
            var temp = await db.GetLatestEnergyReading();
            temp[0].Power_Tot = latestPrecinctEnergy;
            await db.UpdateEnergyMeter(temp[0]);
        }
        #endregion

        #region Services
        public List<ChildDTModel> ReturnChildDTs()
        {
            List<ChildDTModel> childDTList = new List<ChildDTModel>();
            foreach(var building in Buildings)
            {
                var temp = new ChildDTModel(building.Building_name, "Building");
                childDTList.Add(temp);
            }

            return childDTList;
        }
        public async Task<double> ReturnPrecinctLatestEnergyReadingAsync()
        {
            var temp = await db.GetLatestEnergyReading();
            return (double)temp[0].Power_Tot;
        }
        public async Task<List<EnergyMeterModel>> ReturnChildDTEnergyDataAsync(string type, List<string> DTDetailLevel, List<string> dateList)
        {
            List<EnergyMeterModel> energyDataList = new List<EnergyMeterModel>();
            foreach (var DTLevel in DTDetailLevel)
            {
                if (type == "Averages")
                {
                    if (DTLevel == "Building")
                    {
                        foreach (var building in Buildings)
                        {
                            var tempPrecChild = await building.ReturnBuildingEnergyAveragesAsync(dateList);
                            foreach (var item in tempPrecChild)
                            {
                                energyDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "Precinct")
                    {
                        var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList);
                        foreach (var item in tempPrec)
                        {
                            energyDataList.Add(item);
                        }
                    }
                    else if(DTLevel == "All")
                    {
                        foreach (var building in Buildings)
                        {
                            var tempPrecChild = await building.ReturnBuildingEnergyAveragesAsync(dateList);
                            foreach (var item in tempPrecChild)
                            {
                                energyDataList.Add(item);
                            }
                        }
                        var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList);
                        foreach (var item in tempPrec)
                        {
                            energyDataList.Add(item);
                        }
                    }
                }
                else if (type == "CurrentData")
                {
                    if (DTLevel == "Building")
                    {
                        foreach (var building in Buildings)
                        {
                            var tempEnergy = await building.ReturnLatestBuildingEnergyAsync();
                            EnergyMeterModel tempModel = new EnergyMeterModel(building.Building_name, 0, building.Latitude, building.Longitude, tempEnergy, "Latest Reading");
                            energyDataList.Add(tempModel);
                        }
                    }
                    else if (DTLevel == "Precinct")
                    {
                        var tempPrec = await ReturnPrecinctLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, tempPrec, "Latest Reading");
                        energyDataList.Add(temp);
                    }
                    else if (DTLevel == "All")
                    {
                        foreach (var building in Buildings)
                        {
                            var tempEnergy = await building.ReturnLatestBuildingEnergyAsync();
                            EnergyMeterModel tempModel = new EnergyMeterModel(building.Building_name, 0, building.Latitude, building.Longitude, tempEnergy, "Latest Reading");
                            energyDataList.Add(tempModel);
                        }
                        var tempPrec = await ReturnPrecinctLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, tempPrec, "Latest Reading");
                        energyDataList.Add(temp);
                    }
                }
            }
            return energyDataList;
        }
        public async Task<List<EnergyMeterModel>> ReturnPrecinctEnergyAveragesAsync(List<string> dateList)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetEnergyReading(Precinct_name, date);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        public async Task<double> GetTotalEnergyAsync(string date)
        {
            List<string> dates = new List<string>();
            dates.Add(date);
            var temp = await ReturnPrecinctEnergyAveragesAsync(dates);
            double totPower = (double)temp[0].Power_Tot;
            return totPower;
        }
        public async Task<EnergyMeterModel> CalculateEnergyNewAverageAsync(double newPower, string prevTime, string type)
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

            var temp = await db.GetEnergyReading(Precinct_name, prevTimestamp);
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
        private List<InformationModel> GenerateInformationList(List<EnergyMeterModel> energyList)
        {
            List<InformationModel> infoModelList = new List<InformationModel>();
            foreach (var item in energyList)
            {
                string dt_Type = "";
                if (item.EnergyMeter_name != Precinct_name)
                {
                    dt_Type = "Building";
                }
                else
                {
                    dt_Type = "Precinct";
                }
                InformationModel temp = new InformationModel {
                    DataType = "Energy", DT_Type = dt_Type, DT_name = item.EnergyMeter_name, Longitude = item.Longitude, Latitude = item.Latitude
            , Value = (double)item.Power_Tot
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
                    if (message.DisplayType == "Individual")
                    {
                        var tempEnergy = await ReturnPrecinctLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, tempEnergy, "");
                        List<EnergyMeterModel> tempEnergyList = new List<EnergyMeterModel>();
                        tempEnergyList.Add(temp);
                        List<InformationModel> tempList = GenerateInformationList(tempEnergyList);
                        var tempMess = JsonConvert.SerializeObject(tempList);
                        return tempMess;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var tempEnergy = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, null);
                        var infoModelList = GenerateInformationList(tempEnergy);
                        var tempMess = JsonConvert.SerializeObject(infoModelList);
                        return tempMess;
                    }
                }
                else if (message.MessageType == "Averages")
                {
                    message.startDate = utilities.ChangeDateFormat(message.startDate);
                    message.endDate = utilities.ChangeDateFormat(message.endDate);
                    var dateList = utilities.GenerateDateList(message.startDate, message.endDate, message.timePeriod);
                    if (message.DisplayType == "Individual")
                    {
                        var temp = await ReturnPrecinctEnergyAveragesAsync(dateList);
                        var infoModelList = GenerateInformationList(temp);
                        var response = JsonConvert.SerializeObject(infoModelList);
                        return response;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var tempEnergy = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, dateList);
                        var infoModelList = GenerateInformationList(tempEnergy);
                        var tempMess = JsonConvert.SerializeObject(infoModelList);
                        return tempMess;
                    }
                }
            }
            else if (message.DataType == "DigitalTwins")
            {
                if (message.MessageType == "ChildDTList")
                {
                    var temp = ReturnChildDTs();
                    var tempMess = JsonConvert.SerializeObject(temp);
                    return tempMess;
                }
            }

            return "";
        }
        #endregion
    }
}
