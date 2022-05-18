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
using System.Threading;

namespace Precinct_DT
{
    public class Precinct
    {
        public string Precinct_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string IP_Address { get; set; }
        public int Port { get; set; }
        public List<ChildDT> Buildings { get; set; }

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

        private LoadExcel excel = new LoadExcel();

        Services_Communication.ServerSocket myServer = new Services_Communication.ServerSocket();
        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        public Precinct(string name, string latitude, string longitude, string ipAdd, int port, string iniDate)
        {
            Precinct_name = name;
            Latitude = latitude;
            Longitude = longitude;
            IP_Address = ipAdd;
            Port = port;
            db = new PrecinctDBDataAccess(Precinct_name.Replace(" ", "_"));
            _ = db.DeleteDatabase(Precinct_name.Replace(" ", "_"));
            startingDate = iniDate;
        }

        # region Initialisation Functions
        public void InitialisePrecinct()
        {
            Buildings = excel.LoadPrecinctChildren(Precinct_name);
            myServer.SetupServer(Port, null, this, null);
            try
            {              
                while (!Initialised)
                {
                    Thread.Sleep(2000);
                    CheckInitialisations();
                    if (BuildingsInitialised)
                    {
                        foreach (var building in Buildings)
                        {
                            buildingNames.Add(building.ChildDT_Name);
                            MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "LatestEnergy" };
                            var temp = JsonConvert.SerializeObject(tempMes);
                            var response = myClient.sendMessageAsync(temp, building.IP_Address, building.Port).Result;
                            latestPrecinctEnergy += double.Parse(response);
                        }

                        var precinct = new PrecinctModel(Precinct_name, Latitude, Longitude, buildingNames);
                        _ = db.CreatePrecinct(precinct);
                        _ = InitialPopulateDataBaseAsync();
                        Initialised = true;
                    }
                }
                RunPrecinctDT();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Precinct_name} precinct did not initialise");
            }
        }
        private void CheckInitialisations()
        {
            bool initial = true;
            foreach (var building in Buildings)
            {
                MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Status" };
                var temp = JsonConvert.SerializeObject(tempMes);
                var status = myClient.sendMessageAsync(temp, building.IP_Address, building.Port).Result;

                if (status.ToLower() != "true")
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
            await SaveToDataBaseInitialAsync();
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
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
        public void RunPrecinctDT()
        {
            while (true)
            {
                Thread.Sleep(60000);
                //Console.WriteLine("Running " + Precinct_name);
                _ = GetUpdatedDataAsync();
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
                    MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestTimeStamp" };
                    var mes = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
                    dayDate = utilities.DecodeTimestamp(response, "Day");
                    prevEnergyTime = dayDate;

                    tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestEnergy", startDate = dayDate };
                    mes = JsonConvert.SerializeObject(tempMes);
                    response = await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
                    newDayPower += double.Parse(response);
                }
                var temp = await db.GetEnergyReading(Precinct_name, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                /*if (temp != null)
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
                _ = ResetUpdatedDataAvailableAsync();
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
                MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "NewEnergyDataStatus" };
                var temp = JsonConvert.SerializeObject(tempMes);
                var status = myClient.sendMessageAsync(temp, building.IP_Address, building.Port).Result;

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
            foreach (var building in Buildings)
            {
                MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "ResetNewDataAvailable" };
                var mes = JsonConvert.SerializeObject(tempMes);
                await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
            }
        }
        private async Task UpdateLatestEnergyDataAsync()
        {
            // Update latest energy reading
            latestPrecinctEnergy = 0;
            foreach (var building in Buildings)
            {
                MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestEnergy" };
                var mes = JsonConvert.SerializeObject(tempMes);
                var response = await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
                latestPrecinctEnergy += double.Parse(response);
            }
            var temp = await db.GetLatestEnergyReading();
            temp[0].Power_Tot = latestPrecinctEnergy;
            await db.UpdateEnergyMeter(temp[0]);
        }
        private void ResetDataAvailable()
        {
            NewEnergyDataAvailable = false;
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
                    foreach (var building in Buildings)
                    {
                        var temp = new ChildDTModel(building.ChildDT_Name, "Building");
                        childDTList.Add(temp);
                    }

                    return childDTList;
                }
            }            
        }
        public async Task<double> ReturnPrecinctLatestEnergyReadingAsync()
        {
            try
            {
                var temp = await db.GetLatestEnergyReading();
                return (double)temp[0].Power_Tot;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Precinct_name} cannot return latest energy reading");
            }
            return 0;
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
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel { DataType = "Energy", MessageType = "Averages", 
                                startDate = startDate, endDate = endDate, timePeriod = timePeriod };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }                            
                        }
                    }
                    else if (DTLevel == "Precinct")
                    {
                        var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList);
                        var newList = GenerateInformationList(tempPrec);
                        foreach (var item in newList)
                        {
                            informationDataList.Add(item);
                        }
                    }
                    else if (DTLevel == "All")
                    {
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Averages",
                                startDate = startDate, endDate = endDate, timePeriod = timePeriod
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList);
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
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "CurrentData"  };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "Precinct")
                    {
                        var tempPrec = await ReturnPrecinctLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, tempPrec, "Latest Reading");
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
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "CurrentData"
                            };
                            var mes = JsonConvert.SerializeObject(tempMes);
                            var response = await myClient.sendMessageAsync(mes, building.IP_Address, building.Port);
                            var infoList = JsonConvert.DeserializeObject<List<InformationModel>>(response);
                            foreach (var item in infoList)
                            {
                                informationDataList.Add(item);
                            }
                        }

                        var tempPrec = await ReturnPrecinctLatestEnergyReadingAsync();
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, Latitude, Longitude, tempPrec, "Latest Reading");
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
            try
            {
                List<string> dates = new List<string>();
                dates.Add(date);
                var temp = await ReturnPrecinctEnergyAveragesAsync(dates);
                if(temp.Count > 0)
                {
                    double totPower = (double)temp[0].Power_Tot;
                    return totPower;
                }                
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Precinct_name} failed to initialise for {date}");
            };
            return 0;
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
                        var temp = await ReturnPrecinctEnergyAveragesAsync(dateList);
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
            else if (message.DataType == "Initialisation")
            {
                if (message.MessageType == "Status")
                {
                    return Initialised.ToString();
                }
                else if (message.MessageType == "LatestEnergy")
                {
                    var energy = await ReturnPrecinctLatestEnergyReadingAsync();
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
                if (message.MessageType == "LatestTimeStamp")
                {
                    MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestTimeStamp" };
                    var mes = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(mes, Buildings[0].IP_Address, Buildings[0].Port);
                    string dayDate = utilities.DecodeTimestamp(response, "Day");
                    return dayDate;
                }
                else if (message.MessageType == "LatestEnergy")
                {
                    var energy = await ReturnPrecinctLatestEnergyReadingAsync();
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
