using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;
using Models;
using Newtonsoft.Json;
using Utils;

namespace Campus_DT
{
    public class Campus
    {
        public string Campus_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int Port { get; set; }
        public List<Precinct_DT.Precinct> Precincts { get; set; }

        private Precinct_DT.PrecinctManager precinctManager = new Precinct_DT.PrecinctManager();
        private List<string> precinctNames = new List<string>();
        private APICaller apiCaller = new APICaller();
        private Stopwatch stopWatch = new Stopwatch();
        private CampusDBDataAccess db;
        private Utilities utilities = new Utilities();
        private string startingDate;

        private List<EnergyMeterModel> campusInitialEnergyDayReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> campusInitialEnergyMonthReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> campusInitialEnergyYearReadings = new List<EnergyMeterModel>();
        private bool PrecinctsInitialised = false;
        public bool Initialised = false;
        private string prevEnergyTime;
        private bool EnergyDataAvailable = false;

        Services_Communication.ServerSocket myServer = new Services_Communication.ServerSocket();

        public Campus(string name, string latitude, string longitude, int port, string iniDate)
        {
            Campus_name = name;
            Latitude = latitude;
            Longitude = longitude;
            Port = port;
            db = new CampusDBDataAccess(Campus_name.Replace(" ", "_"));
            startingDate = iniDate;
            _ = InitialiseCampusAsync();
        }

        #region Initialisation Functions
        public async Task InitialiseCampusAsync()
        {
            myServer.SetupServer(Port, null, null, this);
            Precincts = precinctManager.InitialisePrecincts(Campus_name, startingDate);            
            while (!Initialised)
            {
                CheckInitialisations();
                if (PrecinctsInitialised)
                {
                    foreach (var prec in Precincts)
                    {
                        precinctNames.Add(prec.Precinct_name);
                    }

                    var campus = new CampusModel(Campus_name, Latitude, Longitude, precinctNames);
                    await db.CreateCampus(campus);
                    await InitialPopulateDataBaseAsync();
                    Initialised = true;
                }
            }
        }
        private void CheckInitialisations()
        {
            bool initial = true;
            foreach (var precincts in Precincts)
            {
                if (precincts.Initialised == false)
                {
                    initial = false;
                }
            }

            if (initial)
            {
                PrecinctsInitialised = true;
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
            prevEnergyTime = utilities.DecodeTimestamp(endDate, "Day");
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
                foreach (var precinct in Precincts)
                {
                    powerTot += await precinct.GetTotalEnergyAsync(date);
                }
                var tempCampusEnergy = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, powerTot, date);
                campusInitialEnergyDayReadings.Add(tempCampusEnergy);
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
                foreach (var precincts in Precincts)
                {
                    powerTot += await precincts.GetTotalEnergyAsync(date);
                }
                var tempCampusEnergy = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, powerTot, date);
                campusInitialEnergyMonthReadings.Add(tempCampusEnergy);
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
                foreach (var precicnts in Precincts)
                {
                    powerTot += await precicnts.GetTotalEnergyAsync(date);
                }
                var tempCampusEnergy = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, powerTot, date);
                campusInitialEnergyYearReadings.Add(tempCampusEnergy);
                powerTot = 0;
            }
        }
        private async Task SaveToDataBaseInitialAsync()
        {
            foreach (var day in campusInitialEnergyDayReadings)
            {
                await db.CreateEnergyReading(day);
            }
            foreach (var month in campusInitialEnergyMonthReadings)
            {
                await db.CreateEnergyReading(month);
            }
            foreach (var year in campusInitialEnergyYearReadings)
            {
                await db.CreateEnergyReading(year);
            }
            Console.WriteLine($"{Campus_name} DT has been initialised");
        }
        #endregion

        #region DT Running Functions
        public void GetUpdatedData()
        {
            Task.Run(() => GetUpdatedEnergyDataAsync());
        }
        private async Task GetUpdatedEnergyDataAsync()
        {
            CheckUpdatedData();
            if (EnergyDataAvailable)
            {                
                double newDayPower = 0;
                string dayDate = "";
                string tempDate = "";
                foreach (var precinct in Precincts)
                {
                    tempDate = precinct.Buildings[0].EnergyMeters[0].latest_timestamp;
                    dayDate = utilities.DecodeTimestamp(precinct.Buildings[0].EnergyMeters[0].latest_timestamp, "Day");
                    prevEnergyTime = dayDate;
                    newDayPower += await precinct.GetTotalEnergyAsync(dayDate);
                }
                var temp = await db.GetEnergyReading(Campus_name, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                if (temp != null)
                {
                    Console.WriteLine($"\nOld day value for energy for {Campus_name} {prevEnergyTime} - {temp[0].Power_Tot}");
                }
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
                    var tempDayMeter = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, (double)newDayEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                    var tempMonthMeter = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, (double)newMonthEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Month"));
                    var tempYearMeter = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, (double)newYearEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Year"));
                    await db.CreateEnergyReading(tempDayMeter);
                    await db.CreateEnergyReading(tempMonthMeter);
                    await db.CreateEnergyReading(tempYearMeter);
                }
                ResetUpdatedDataAvailable();
                temp = await db.GetEnergyReading(Campus_name, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                if (temp != null)
                {
                    Console.WriteLine($"Updated day value for energy for {Campus_name} {tempDate} - {temp[0].Power_Tot}");
                }
            }
        }
        private void CheckUpdatedData()
        {
            bool initial = true;
            foreach (var precincts in Precincts)
            {
                if (precincts.NewEnergyDataAvailable == false)
                {
                    initial = false;
                }
            }
            if (initial)
            {
                EnergyDataAvailable = true;
            }
        }
        private void ResetUpdatedDataAvailable()
        {
            EnergyDataAvailable = false;
            foreach (var precincts in Precincts)
            {
                precincts.NewEnergyDataAvailable = false;
            }
        }
        #endregion

        #region Services
        public List<ChildDTModel> ReturnChildDTs()
        {
            List<ChildDTModel> childDTList = new List<ChildDTModel>();
            while (true)
            {
                if (Initialised)
                {
                    foreach (var precinct in Precincts)
                    {
                        var temp = new ChildDTModel(precinct.Precinct_name, "Precinct");
                        childDTList.Add(temp);
                    }

                    return childDTList;
                }
            }
        }
        private double ReturnCurrentEnergyReading()
        {
            double currentPower = 0;

            foreach (var precincts in Precincts)
            {
                currentPower += precincts.ReturnPrecinctCurrentEnergyReading();
            }

            return currentPower;
        }
        public async Task<List<EnergyMeterModel>> ReturnChildDTEnergyDataAsync(string type, string DTLevel, List<string> dateList)
        {
            List<EnergyMeterModel> energyDataList = new List<EnergyMeterModel>();
            if (type == "Averages")
            {
                if(DTLevel == "Building")
                {
                    foreach(var precinct in Precincts)
                    {
                        var temp = await precinct.ReturnChildDTEnergyDataAsync(type, DTLevel, dateList);
                        foreach (var item in temp)
                        {
                            energyDataList.Add(item);
                        }
                    }
                }
                else if(DTLevel == "Precinct")
                {
                    foreach (var precinct in Precincts)
                    {
                        var temp = await precinct.ReturnPrecinctEnergyAveragesAsync(dateList);
                        foreach (var item in temp)
                        {
                            energyDataList.Add(item);
                        }
                    }
                }
                else if(DTLevel == "All")
                {
                    foreach (var precinct in Precincts)
                    {
                        var tempPrecChild = await precinct.ReturnChildDTEnergyDataAsync(type, DTLevel, dateList);
                        foreach (var item in tempPrecChild)
                        {
                            energyDataList.Add(item);
                        }
                        var tempPrec = await precinct.ReturnPrecinctEnergyAveragesAsync(dateList);
                        foreach (var item in tempPrec)
                        {
                            energyDataList.Add(item);
                        }                        
                    }
                    var tempCampus = await ReturnEnergyAveragesAsync(dateList);
                    foreach (var item in tempCampus)
                    {
                        energyDataList.Add(item);
                    }
                }
                else if (DTLevel == "Campus")
                {
                    foreach (var precinct in Precincts)
                    {
                        var temp = await precinct.ReturnPrecinctEnergyAveragesAsync(dateList);
                        foreach (var item in temp)
                        {
                            energyDataList.Add(item);
                        }
                    }
                }                
            }
            else if (type == "CurrentData")
            {
                if (DTLevel == "Building")
                {
                    foreach (var precinct in Precincts)
                    {
                        var temp = await precinct.ReturnChildDTEnergyDataAsync(type, DTLevel, dateList);
                        foreach (var item in temp)
                        {
                            energyDataList.Add(item);
                        }
                    }
                }
                else if (DTLevel == "Precinct")
                {
                    foreach (var precinct in Precincts)
                    {
                        var tempPrecinct = precinct.ReturnPrecinctCurrentEnergyReading();
                        EnergyMeterModel temp = new EnergyMeterModel(Campus_name, 0, Latitude, Latitude, tempPrecinct, "");
                        energyDataList.Add(temp);
                    }
                }
                else if (DTLevel == "All")
                {
                    foreach (var precinct in Precincts)
                    {
                        var tempPrecChild = await precinct.ReturnChildDTEnergyDataAsync(type, DTLevel, dateList);
                        foreach (var item in tempPrecChild)
                        {
                            energyDataList.Add(item);
                        }
                    }
                    var tempCampus = ReturnCurrentEnergyReading();
                    EnergyMeterModel temp = new EnergyMeterModel(Campus_name, 0, Latitude, Latitude, tempCampus, "");
                    energyDataList.Add(temp);
                }
                else if (DTLevel == "Campus")
                {
                    foreach (var precinct in Precincts)
                    {
                        var tempEnergy = precinct.ReturnPrecinctCurrentEnergyReading();
                        EnergyMeterModel temp = new EnergyMeterModel(precinct.Precinct_name, 0, precinct.Latitude, precinct.Latitude, tempEnergy, "");
                        energyDataList.Add(temp);
                    }
                }
            }
            return energyDataList;
        }
        private async Task<List<EnergyMeterModel>> ReturnEnergyAveragesAsync(List<string> dateList)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetEnergyReading(Campus_name, date);
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
            var temp = await ReturnEnergyAveragesAsync(dates);
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

            var temp = await db.GetEnergyReading(Campus_name, prevTimestamp);
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
        public async Task<string> ServiceHandlerAsync(MessageModel message)
        {
            if (message.DataType == "Energy")
            {
                if (message.MessageType == "CurrentData")
                {
                    if (message.DisplayType == "Individual")
                    {
                        var tempEnergy = ReturnCurrentEnergyReading();
                        EnergyMeterModel temp = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, tempEnergy, "");
                        var tempMess = JsonConvert.SerializeObject(temp);
                        return tempMess;
                    }
                    else if(message.DisplayType == "Collective")
                    {
                        var tempEnergy = await ReturnChildDTEnergyDataAsync(message.MessageType, message.LowestDTLevel, null);
                        var tempMess = JsonConvert.SerializeObject(tempEnergy);
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
                        var temp = await ReturnEnergyAveragesAsync(dateList);
                        var response = JsonConvert.SerializeObject(temp);
                        return response;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var tempEnergy = await ReturnChildDTEnergyDataAsync(message.MessageType, message.LowestDTLevel, dateList);
                        var tempMess = JsonConvert.SerializeObject(tempEnergy);
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
