using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        public string IP_Address { get; set; }
        public int Port { get; set; }
        public List<ChildDT> Precincts { get; set; }

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
        private double latestCampusEnergy = 0;

        private LoadExcel excel = new LoadExcel();

        Services_Communication.ServerSocket myServer = new Services_Communication.ServerSocket();
        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        public Campus(string name, string latitude, string longitude, string ipAdd, int port, string iniDate)
        {
            Campus_name = name;
            Latitude = latitude;
            Longitude = longitude;
            IP_Address = ipAdd;
            Port = port;
            db = new CampusDBDataAccess(Campus_name.Replace(" ", "_"));
            _ = db.DeleteDatabase(Campus_name.Replace(" ", "_"));
            startingDate = iniDate;
        }

        #region Initialisation Functions
        public void InitialiseCampus()
        {
            Precincts = excel.LoadCampusChildren(Campus_name);
            myServer.SetupServer(Port, null, null, this);
            while (!Initialised)
            {
                Thread.Sleep(2000);
                CheckInitialisations();
                if (PrecinctsInitialised)
                {
                    foreach (var precinct in Precincts)
                    {
                        precinctNames.Add(precinct.ChildDT_Name);
                        MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "LatestEnergy" };
                        var temp = JsonConvert.SerializeObject(tempMes);
                        var response = myClient.sendMessageAsync(temp, precinct.IP_Address, precinct.Port).Result;
                        latestCampusEnergy += double.Parse(response);
                    }

                    var campus = new CampusModel(Campus_name, Latitude, Longitude, precinctNames);
                    _ = db.CreateCampus(campus);
                    _ = InitialPopulateDataBaseAsync();
                    Initialised = true;
                }
            }
            RunCampusDT();
        }
        private void CheckInitialisations()
        {
            bool initial = true;
            foreach (var precinct in Precincts)
            {
                MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Status" };
                var temp = JsonConvert.SerializeObject(tempMes);
                var status = myClient.sendMessageAsync(temp, precinct.IP_Address, precinct.Port).Result;

                if (status.ToLower() != "true")
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
            await SaveToDataBaseInitialAsync();
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, precinct.IP_Address, precinct.Port);
                    powerTot += double.Parse(response);
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
                foreach (var precinct in Precincts)
                {
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, precinct.IP_Address, precinct.Port);
                    powerTot += double.Parse(response);
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
                foreach (var precinct in Precincts)
                {
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, precinct.IP_Address, precinct.Port);
                    powerTot += double.Parse(response);
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
            var tempCurrent = new EnergyMeterModel("Current", 0, Latitude, Longitude, latestCampusEnergy, "");
            await db.CreateEnergyReading(tempCurrent);
            Console.WriteLine($"{Campus_name} DT has been initialised");
        }
        #endregion

        #region DT Running Functions
        public void RunCampusDT()
        {
            while (true)
            {
                Thread.Sleep(60000);
                //Console.WriteLine("Running " + Precinct_name);
                _ = GetUpdatedDataAsync();
            }
        }
        public async Task GetUpdatedDataAsync()
        {
            await GetUpdatedEnergyDataAsync();
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
                    MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestTimeStamp" };
                    var mes = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                    dayDate = utilities.DecodeTimestamp(response, "Day");
                    prevEnergyTime = dayDate;

                    tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestEnergy", startDate = dayDate };
                    mes = JsonConvert.SerializeObject(tempMes);
                    response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                    newDayPower += double.Parse(response);
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
                _ = ResetUpdatedDataAvailableAsync();
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
            foreach (var precinct in Precincts)
            {
                MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "NewEnergyDataStatus" };
                var temp = JsonConvert.SerializeObject(tempMes);
                var status = myClient.sendMessageAsync(temp, precinct.IP_Address, precinct.Port).Result;

                if (status.ToLower() == "false")
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
        private async Task ResetUpdatedDataAvailableAsync()
        {
            EnergyDataAvailable = false;
            foreach (var precinct in Precincts)
            {
                MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "ResetNewDataAvailable" };
                var mes = JsonConvert.SerializeObject(tempMes);
                await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
            }
        }
        private async Task UpdateLatestEnergyDataAsync()
        {
            // Update latest energy reading
            latestCampusEnergy = 0;
            foreach (var precinct in Precincts)
            {
                MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestEnergy" };
                var mes = JsonConvert.SerializeObject(tempMes);
                var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                latestCampusEnergy += double.Parse(response);
            }
            var temp = await db.GetLatestEnergyReading();
            temp[0].Power_Tot = latestCampusEnergy;
            await db.UpdateEnergyMeter(temp[0]);
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
                        var temp = new ChildDTModel(precinct.ChildDT_Name, "Precinct");
                        childDTList.Add(temp);
                    }

                    return childDTList;
                }
            }
        }
        private async Task<double> ReturnLatestEnergyReadingAsync()
        {
            var temp = await db.GetLatestEnergyReading();
            return (double)temp[0].Power_Tot;
        }
        public async Task<List<InformationModel>> ReturnChildDTEnergyDataAsync(string type, List<string> DTDetailLevel, string startDate, string endDate, string timePeriod)
        {
            List<InformationModel> informationDataList = new List<InformationModel>();
            string stDate = utilities.ChangeDateFormat(startDate);
            string enDate = utilities.ChangeDateFormat(endDate);
            var dateList = utilities.GenerateDateList(stDate, enDate, timePeriod);
            foreach (var DTLevel in DTDetailLevel)
            {
                if (type == "Averages")
                {
                    if (DTLevel == "Building")
                    {
                        foreach (var precinct in Precincts)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Averages", DTDetailLevel = DTDetailLevel,
                                startDate = startDate, endDate = endDate, timePeriod = timePeriod
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "Precinct")
                    {
                        foreach (var precinct in Precincts)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Averages", DTDetailLevel = DTDetailLevel,
                                startDate = startDate, endDate = endDate, timePeriod = timePeriod
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "Campus")
                    {
                        var tempPrec = await ReturnEnergyAveragesAsync(dateList);
                        var newList = GenerateInformationList(tempPrec);
                        foreach (var item in newList)
                        {
                            informationDataList.Add(item);
                        }
                    }
                    else if (DTLevel == "All")
                    {
                        foreach (var precinct in Precincts)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Averages", DTDetailLevel = DTDetailLevel,
                                startDate = startDate, endDate = endDate, timePeriod = timePeriod
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        var tempPrec = await ReturnEnergyAveragesAsync(dateList);
                        var newList = GenerateInformationList(tempPrec);
                        foreach (var item in newList)
                        {
                            informationDataList.Add(item);
                        }
                    }
                }
                else if (type == "CurrentData")
                {
                    if (DTLevel == "Building")
                    {
                        foreach (var precinct in Precincts)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "CurrentData", DTDetailLevel = DTDetailLevel
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }                        
                    }
                    else if (DTLevel == "Precinct")
                    {
                        foreach (var precinct in Precincts)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "CurrentData", DTDetailLevel = DTDetailLevel
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "Campus")
                    {
                        var tempPrec = await ReturnLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, tempPrec, "Latest Reading");
                        var tempEnergyList = new List<EnergyMeterModel>();
                        tempEnergyList.Add(temp);
                        var newList = GenerateInformationList(tempEnergyList);
                        foreach (var item in newList)
                        {
                            informationDataList.Add(item);
                        }
                    }
                    else if (DTLevel == "All")
                    {
                        foreach (var precinct in Precincts)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "CurrentData", DTDetailLevel = DTDetailLevel
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, precinct.IP_Address, precinct.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        var tempPrec = await ReturnLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, tempPrec, "Latest Reading");
                        var tempEnergyList = new List<EnergyMeterModel>();
                        tempEnergyList.Add(temp);
                        var newList = GenerateInformationList(tempEnergyList);
                        foreach (var item in newList)
                        {
                            informationDataList.Add(item);
                        }
                    }
                }
            }
            return informationDataList;
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
        private List<InformationModel> GenerateInformationList(List<EnergyMeterModel> energyList)
        {
            List<InformationModel> infoModelList = new List<InformationModel>();
            foreach (var item in energyList)
            {
                string dt_Type = "";
                if (item.EnergyMeter_name != Campus_name)
                {

                    foreach (var prec in Precincts)
                    {
                        if (item.EnergyMeter_name == prec.ChildDT_Name)
                        {
                            dt_Type = "Precinct";
                            break;
                        }
                        else
                        {
                            dt_Type = "Building";
                        }
                    }
                }
                else
                {
                    dt_Type = "Campus";
                }
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
                    if (message.DisplayType == "Individual")
                    {
                        var tempEnergy = await ReturnLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Campus_name, 0, Latitude, Longitude, tempEnergy, "");
                        List<EnergyMeterModel> tempEnergyList = new List<EnergyMeterModel>();
                        tempEnergyList.Add(temp);
                        List<InformationModel> tempList = GenerateInformationList(tempEnergyList);
                        var tempMess = JsonConvert.SerializeObject(tempList);
                        return tempMess;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var infoModelList = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, null, null, null);
                        var tempMess = JsonConvert.SerializeObject(infoModelList);
                        return tempMess;
                    }
                }
                else if (message.MessageType == "Averages")
                {
                    if (message.DisplayType == "Individual")
                    {
                        string stDate = utilities.ChangeDateFormat(message.startDate);
                        string enDate = utilities.ChangeDateFormat(message.endDate);
                        var dateList = utilities.GenerateDateList(stDate, enDate, message.timePeriod);
                        var temp = await ReturnEnergyAveragesAsync(dateList);
                        var infoModelList = GenerateInformationList(temp);
                        var response = JsonConvert.SerializeObject(infoModelList);
                        return response;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var infoModelList = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, message.startDate, message.endDate, message.timePeriod);
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
