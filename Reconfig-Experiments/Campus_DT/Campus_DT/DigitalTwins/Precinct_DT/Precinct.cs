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

        private EnergyMeterData precinctEnergyMeter = new EnergyMeterData();
        private EnergyMeters energyManager = new EnergyMeters();

        private List<EnergyMeterModel> precinctInitialEnergyDayReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyMonthReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyYearReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyMaxDayReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyMaxMonthReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyMaxYearReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyTotalDayReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyTotalMonthReadings = new List<EnergyMeterModel>();
        private List<EnergyMeterModel> precinctInitialEnergyTotalYearReadings = new List<EnergyMeterModel>();
        private bool BuildingsInitialised = false;
        private string prevEnergyTime;
        private bool EnergyDataAvailable = false;
        private double latestPrecinctEnergy = 0;
        private double mainLatestPrecinctEnergy = 0;

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
            precinctEnergyMeter = excel.LoadEnergyMeterData(Precinct_name)[0];
            myServer.SetupServer(Port, null, this, null);
            try
            {              
                while (!Initialised)
                {
                    Thread.Sleep(2000);
                    CheckInitialisations();
                    if (Buildings.Count > 0)
                    {
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
                            var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Meter", precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, 0, null);
                            _ = db.CreateEnergyReading(tempMeter);

                            var precinct = new PrecinctModel(Precinct_name, Latitude, Longitude, buildingNames);
                            _ = db.CreatePrecinct(precinct);
                            _ = InitialPopulateDataBaseAsync();
                            Initialised = true;
                        }
                    }
                    else
                    {
                        var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Meter", precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, 0, null);
                        _ = db.CreateEnergyReading(tempMeter);

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
                Console.WriteLine($"{e} {Precinct_name} precinct did not initialise");
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
            InitialiseEnergyMeterData();
            await InitialContextGenerationAsync(startingDate, apiCaller.GetCurrentDateTime().Item1);
            await SaveToDataBaseInitialAsync();
        }
        private void InitialiseEnergyMeterData()
        {
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.data = energyManager.GetMeterData(startingDate, apiCaller.GetCurrentDateTime().Item1, precinctEnergyMeter.meterid);
                if (precinctEnergyMeter.data.Count > 0)
                {
                    precinctEnergyMeter.latest_power = precinctEnergyMeter.data[precinctEnergyMeter.data.Count - 1].ptot_kw;
                    precinctEnergyMeter.latest_timestamp = precinctEnergyMeter.data[precinctEnergyMeter.data.Count - 1].timestamp;
                    mainLatestPrecinctEnergy += precinctEnergyMeter.latest_power;
                }
            }
        }
        private async Task InitialContextGenerationAsync(String startDate, String endDate)
        {
            await CalculateInitialEnergyDayAverageAsync(startDate, endDate);
            await CalculateInitialEnergyMonthAverage(startDate, endDate);
            await CalculateInitialEnergyYearAverage(startDate, endDate);
            await CalculateInitialEnergyDayMaxAsync(startDate, endDate);
            await CalculateInitialEnergyMonthMaxAsync(startDate, endDate);
            await CalculateInitialEnergyYearMaxAsync(startDate, endDate);
            await CalculateInitialEnergyDayTotalAsync(startDate, endDate);
            await CalculateInitialEnergyMonthTotalAsync(startDate, endDate);
            await CalculateInitialEnergyYearTotalAsync(startDate, endDate);
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date, timePeriod = "Day" };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Day Average", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyDayReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.day_average = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Day").day_average;
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date, timePeriod = "Month" };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Month Average", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyMonthReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.month_average = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Month").month_average;
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Averages", startDate = date, timePeriod = "Year" };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Year Average", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyYearReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.year_average = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Year").year_average;
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
                int count = 0;
                double maxEnergy = 0;
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
                                if (data.ptot_kw > maxEnergy)
                                {
                                    maxEnergy = data.ptot_kw;
                                }
                                tempEnergy += data.difference_imp_kwh;
                                count++;
                            }
                        }

                        tempData = new EnergyAverageData();
                        tempData.timestamp = year + "-" + month + "-" + day;
                        tempData.ptot_kw = tempEnergy / count;
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
                int count = 0;
                double maxEnergy = 0;
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
                        tempData.ptot_kw = tempEnergy / count;
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
                int count = 0;
                double maxEnergy = 0;
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
                        tempData.ptot_kw = tempEnergy / count;
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
        public async Task CalculateInitialEnergyDayMaxAsync(string startDate, string endDate)
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Max", startDate = date, timePeriod = type };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Day Max", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyMaxDayReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.day_max = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Day").day_max;
            }
        }
        public async Task CalculateInitialEnergyMonthMaxAsync(string startDate, string endDate)
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Max", startDate = date, timePeriod = type };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Month Max", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyMaxMonthReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.month_max = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Month").month_max;
            }
        }
        public async Task CalculateInitialEnergyYearMaxAsync(string startDate, string endDate)
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Max", startDate = date, timePeriod = type };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Year Max", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyMaxYearReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.year_max = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Day").year_max;
            }
        }
        public async Task CalculateInitialEnergyDayTotalAsync(string startDate, string endDate)
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Total", startDate = date, timePeriod = type };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Day Total", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyTotalDayReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.day_max = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Day").day_tot;
            }
        }
        public async Task CalculateInitialEnergyMonthTotalAsync(string startDate, string endDate)
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Total", startDate = date, timePeriod = type };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Month Total", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyTotalMonthReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.month_max = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Month").month_tot;
            }
        }
        public async Task CalculateInitialEnergyYearTotalAsync(string startDate, string endDate)
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
                    MessageModel tempMes = new MessageModel { DataType = "Initialisation", MessageType = "Total", startDate = date, timePeriod = type };
                    var temp = JsonConvert.SerializeObject(tempMes);
                    var response = await myClient.sendMessageAsync(temp, building.IP_Address, building.Port);
                    powerTot += double.Parse(response);
                }
                var tempPrecinctEnergy = new EnergyMeterModel(Precinct_name, 0, "Year Total", Latitude, Longitude, powerTot, date);
                precinctInitialEnergyTotalYearReadings.Add(tempPrecinctEnergy);
                powerTot = 0;
            }
            if (precinctEnergyMeter != null)
            {
                precinctEnergyMeter.year_max = CalculateAverages(precinctEnergyMeter, startDate, endDate, "Day").year_tot;
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
            foreach (var day in precinctInitialEnergyMaxDayReadings)
            {
                await db.CreateEnergyReading(day);
            }
            foreach (var month in precinctInitialEnergyMaxMonthReadings)
            {
                await db.CreateEnergyReading(month);
            }
            foreach (var year in precinctInitialEnergyMaxYearReadings)
            {
                await db.CreateEnergyReading(year);
            }
            foreach (var day in precinctInitialEnergyTotalDayReadings)
            {
                await db.CreateEnergyReading(day);
            }
            foreach (var month in precinctInitialEnergyTotalMonthReadings)
            {
                await db.CreateEnergyReading(month);
            }
            foreach (var year in precinctInitialEnergyTotalYearReadings)
            {
                await db.CreateEnergyReading(year);
            }
            var tempCurrent = new EnergyMeterModel("Current", 0, "Current", Latitude, Longitude, latestPrecinctEnergy, "");
            await db.CreateEnergyReading(tempCurrent);
            if (precinctEnergyMeter != null)
            {
                for (int i = 0; i < precinctEnergyMeter.day_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Day Average", 
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.day_average[i].ptot_kw, 
                        precinctEnergyMeter.day_average[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.month_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Month Average",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.month_average[i].ptot_kw,
                        precinctEnergyMeter.month_average[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.year_average.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Year Average",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.year_average[i].ptot_kw,
                        precinctEnergyMeter.year_average[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.day_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Day Max",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.day_max[i].ptot_kw,
                        precinctEnergyMeter.day_max[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.month_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Month Max",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.month_max[i].ptot_kw,
                        precinctEnergyMeter.month_max[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.year_max.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Year Max",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.year_max[i].ptot_kw,
                        precinctEnergyMeter.year_max[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.day_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Day Total",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.day_tot[i].ptot_kw,
                        precinctEnergyMeter.day_tot[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.month_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Month Total",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.month_tot[i].ptot_kw,
                        precinctEnergyMeter.month_tot[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                for (int i = 0; i < precinctEnergyMeter.year_tot.Count; i++)
                {
                    var tempMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Year Total",
                        precinctEnergyMeter.latitude, precinctEnergyMeter.longitude, precinctEnergyMeter.year_tot[i].ptot_kw,
                        precinctEnergyMeter.year_tot[i].timestamp);
                    await db.CreateEnergyReading(tempMeter);
                }
                tempCurrent = new EnergyMeterModel("MainMeterCurrent", precinctEnergyMeter.meterid, "Main Current", Latitude, Longitude, mainLatestPrecinctEnergy, "");
                await db.CreateEnergyReading(tempCurrent);
            }
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
            if (Buildings.Count > 0)
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
                    //var temp = await db.GetEnergyReading(Precinct_name, utilities.DecodeTimestamp(prevEnergyTime, "Day"), 0, null);
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
                        var tempDayMeter = new EnergyMeterModel(Precinct_name, 0, "Day Average", Latitude, Longitude, (double)newDayEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Day"));
                        var tempMonthMeter = new EnergyMeterModel(Precinct_name, 0, "Month Average", Latitude, Longitude, (double)newMonthEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Month"));
                        var tempYearMeter = new EnergyMeterModel(Precinct_name, 0, "Year Average", Latitude, Longitude, (double)newYearEnergyData.Power_Tot, utilities.DecodeTimestamp(prevEnergyTime, "Year"));
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
            else
            {
                await GetCurrentEnergyMeterDataAsync();
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
        private async Task GetCurrentEnergyMeterDataAsync()
        {
            var tempData = energyManager.GetCurrentEnergyData(precinctEnergyMeter.meterid);
            if (tempData.Count > 0)
            {
                if (tempData[tempData.Count - 1].timestamp != precinctEnergyMeter.latest_timestamp)
                {
                    precinctEnergyMeter.previous_timestamp = precinctEnergyMeter.latest_timestamp;
                    precinctEnergyMeter.latest_timestamp = tempData[tempData.Count - 1].timestamp;
                    precinctEnergyMeter.latest_power = tempData[tempData.Count - 1].ptot_kw;
                    /*var temp = await db.GetEnergyMeterReading(item.meterid, utilities.DecodeTimestamp(item.previous_timestamp, "Day"));
                    if (temp != null)
                    {
                        Console.WriteLine($"Old day value for energy for {item.description} {item.previous_timestamp} - {temp[0].Power_Tot}");
                    }*/
                    var newDayEnergyData = await CalculateEnergyNewAverageAsync(precinctEnergyMeter.meterid, precinctEnergyMeter.latest_power, precinctEnergyMeter.previous_timestamp, "Day");
                    var newMonthEnergyData = await CalculateEnergyNewAverageAsync(precinctEnergyMeter.meterid, precinctEnergyMeter.latest_power, precinctEnergyMeter.previous_timestamp, "Month");
                    var newYearEnergyData = await CalculateEnergyNewAverageAsync(precinctEnergyMeter.meterid, precinctEnergyMeter.latest_power, precinctEnergyMeter.previous_timestamp, "Year");
                    if (newDayEnergyData.Timestamp != null)
                    {
                        await db.UpdateEnergyMeter(newDayEnergyData);
                        await db.UpdateEnergyMeter(newMonthEnergyData);
                        await db.UpdateEnergyMeter(newYearEnergyData);
                    }
                    else if (newDayEnergyData.Timestamp == null)
                    {
                        var tempDayMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Day Average", precinctEnergyMeter.latitude,
                            precinctEnergyMeter.longitude, precinctEnergyMeter.latest_power, utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Day"));
                        await db.CreateEnergyReading(tempDayMeter);

                        var prevMonth = utilities.DecodeTimestamp(newDayEnergyData.Timestamp, "Month");
                        var prevYear = utilities.DecodeTimestamp(newDayEnergyData.Timestamp, "Year");

                        if (utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Month") == prevMonth)
                        {
                            await db.UpdateEnergyMeter(newMonthEnergyData);
                        }
                        else if (utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Month") != prevMonth)
                        {
                            var tempMonthMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Month Average", precinctEnergyMeter.latitude,
                                precinctEnergyMeter.longitude, precinctEnergyMeter.latest_power, utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Month"));
                            await db.CreateEnergyReading(tempMonthMeter);
                        }

                        if (utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Year") == prevYear)
                        {
                            await db.UpdateEnergyMeter(newYearEnergyData);
                        }
                        else if (utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Year") != prevYear)
                        {
                            var tempYearMeter = new EnergyMeterModel(precinctEnergyMeter.description, precinctEnergyMeter.meterid, "Year Average", precinctEnergyMeter.latitude,
                                precinctEnergyMeter.longitude, precinctEnergyMeter.latest_power, utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Year"));
                            await db.CreateEnergyReading(tempYearMeter);
                        }
                    }
                    precinctEnergyMeter.NewDataAvailable = true;
                    //temp = await db.GetEnergyMeterReading(item.meterid, utilities.DecodeTimestamp(item.latest_timestamp, "Day"));
                    //Console.WriteLine($"Updated day value for energy for {item.description} {item.latest_timestamp} - {temp[0].Power_Tot}");
                    //Console.WriteLine(Building_name + " Updated");
                }
            }
            if(Buildings.Count == 0) {
                NewEnergyDataAvailable = true;
            }            
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

            var temp = await db.GetEnergyReading(Precinct_name, prevTimestamp, meterid, null);
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
            List<EnergyMeterModel> temp = new List<EnergyMeterModel>();
            if(Buildings.Count > 0)
            {
                temp = await db.GetLatestEnergyReading(0);
                temp[0].Power_Tot = latestPrecinctEnergy;
            }
            else
            {
                temp = await db.GetLatestEnergyReading(precinctEnergyMeter.meterid);
                var data = energyManager.GetCurrentEnergyData(precinctEnergyMeter.meterid);
                temp[0].Power_Tot = data[data.Count - 1].ptot_kw;
            }
            
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
                List<EnergyMeterModel> temp = new List<EnergyMeterModel>();
                if(Buildings.Count > 0)
                {
                    temp = await db.GetLatestEnergyReading(0);
                }
                else
                {
                    temp = await db.GetLatestEnergyReading(precinctEnergyMeter.meterid);
                }
                return (double)temp[0].Power_Tot;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Precinct_name} cannot return latest energy reading");
            }
            return 0;
        }
        public async Task<List<InformationModel>> ReturnChildDTEnergyDataAsync(string type, List<string> DTDetailLevel, string displayType, string startDate, string endDate, string timePeriod)
        {
            List<InformationModel> informationDataList = new List<InformationModel>();
            List<string> dateList = new List<string>();
            if ((startDate != null) || (endDate != null) || (timePeriod != null))
            {
                string stDate = utilities.ChangeDateFormat(startDate);
                string enDate = utilities.ChangeDateFormat(endDate);
                dateList = utilities.GenerateDateList(stDate, enDate, timePeriod);
            }
            foreach (var DTLevel in DTDetailLevel)
            {
                if (type == "Averages")
                {
                    if (DTLevel == "Building")
                    {
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel { DataType = "Energy", MessageType = "Averages", DisplayType = displayType, 
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
                        if (Buildings.Count > 0)
                        {
                            var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList, 0, $"{timePeriod} Average");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        else
                        {
                            var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList, precinctEnergyMeter.meterid, $"{timePeriod} Average");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "All")
                    {
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Averages", DisplayType = displayType,
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
                        if (Buildings.Count > 0)
                        {
                            var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList, 0, $"{timePeriod} Average");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        else
                        {
                            var tempPrec = await ReturnPrecinctEnergyAveragesAsync(dateList, precinctEnergyMeter.meterid, $"{timePeriod} Average");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
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
                                DataType = "Energy", MessageType = "CurrentData", DisplayType = displayType };
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
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, "Current", Latitude, Longitude, tempPrec, "Latest Reading");
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
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, "Current", Latitude, Longitude, tempPrec, "Latest Reading");
                        var tempEnergyList = new List<EnergyMeterModel>();
                        tempEnergyList.Add(temp);
                        var newList = GenerateInformationList(tempEnergyList);
                        foreach (var item in newList)
                        {
                            informationDataList.Add(item);
                        }
                    }
                }
                else if (type == "Max")
                {
                    if (DTLevel == "Building")
                    {
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Max", DisplayType = displayType,
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
                    }
                    else if (DTLevel == "Precinct")
                    {
                        if (Buildings.Count > 0)
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, 0, $"{type} Max");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        else
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, precinctEnergyMeter.meterid, $"{type} Max");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "All")
                    {
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Max", DisplayType = displayType,
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
                        if (Buildings.Count > 0)
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, 0, $"{timePeriod} Max");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        else
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, precinctEnergyMeter.meterid, $"{type} Max");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                }
                else if (type == "Total")
                {
                    if (DTLevel == "Building")
                    {
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Total", DisplayType = displayType,
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
                    }
                    else if (DTLevel == "Precinct")
                    {
                        if (Buildings.Count > 0)
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, 0, $"{type} Total");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        else
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, precinctEnergyMeter.meterid, $"{type} Total");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                    else if (DTLevel == "All")
                    {
                        foreach (var building in Buildings)
                        {
                            MessageModel tempMes = new MessageModel {
                                DataType = "Energy", MessageType = "Total", DisplayType = displayType,
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
                        if (Buildings.Count > 0)
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, 0, $"{timePeriod} Total");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                        else
                        {
                            var tempPrec = await ReturnPrecinctEnergyDataAsync(dateList, precinctEnergyMeter.meterid, $"{type} Total");
                            var newList = GenerateInformationList(tempPrec);
                            foreach (var item in newList)
                            {
                                informationDataList.Add(item);
                            }
                        }
                    }
                }
            }
            return informationDataList;
        }
        public async Task<List<EnergyMeterModel>> ReturnPrecinctEnergyAveragesAsync(List<string> dateList, int meterid, string type)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetEnergyReading(Precinct_name, date, meterid, type);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        public async Task<List<EnergyMeterModel>> ReturnPrecinctEnergyDataAsync(List<string> dateList, int meterid, string type)
        {
            List<EnergyMeterModel> meterData = new List<EnergyMeterModel>();
            foreach (var date in dateList)
            {
                var temp = await db.GetEnergyReading(Precinct_name, date, meterid, type);
                if (temp.Count != 0)
                {
                    meterData.Add(temp[0]);
                }
            }

            return meterData;
        }
        public async Task<double> GetTotalEnergyAsync(string date, string type)
        {
            try
            {
                List<string> dates = new List<string>();
                dates.Add(date);
                var temp = await ReturnPrecinctEnergyAveragesAsync(dates, 0, type);
                if(temp.Count > 0)
                {
                    double totPower = (double)temp[0].Power_Tot;
                    return totPower;
                }                
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Precinct_name} failed to initialise for {date} for {type}");
            };
            return 0;
        }
        public async Task<double> GetTotalEnergyDataAsync(string date, string type)
        {
            try
            {
                List<string> dates = new List<string>();
                dates.Add(date);
                var temp = await ReturnPrecinctEnergyDataAsync(dates, 0, type);
                if (temp.Count > 0)
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

            var temp = await db.GetEnergyReading(Precinct_name, prevTimestamp, 0, null);
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
                        EnergyMeterModel temp = new EnergyMeterModel(Precinct_name, 0, "Current", Latitude, Longitude, tempEnergy, "");
                        List<EnergyMeterModel> tempEnergyList = new List<EnergyMeterModel>();
                        tempEnergyList.Add(temp);
                        List<InformationModel> tempList = GenerateInformationList(tempEnergyList);
                        var tempMess = JsonConvert.SerializeObject(tempList);
                        return tempMess;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var infoModelList = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, message.DisplayType, null, null, null);
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
                        var temp = new List<EnergyMeterModel>();
                        if(Buildings.Count > 0)
                        {
                            temp = await ReturnPrecinctEnergyAveragesAsync(dateList, 0, $"{message.timePeriod} Max");
                        }
                        else
                        {
                            temp = await ReturnPrecinctEnergyAveragesAsync(dateList, precinctEnergyMeter.meterid, $"{message.timePeriod} Max");
                        }
                        var infoModelList = GenerateInformationList(temp);
                        var response = JsonConvert.SerializeObject(infoModelList);
                        return response;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var infoModelList = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, message.DisplayType, message.startDate, message.endDate, message.timePeriod);
                        var tempMess = JsonConvert.SerializeObject(infoModelList);
                        return tempMess;
                    }
                }
                else if (message.MessageType == "Max")
                {
                    if (message.DisplayType == "Individual")
                    {
                        string stDate = utilities.ChangeDateFormat(message.startDate);
                        string enDate = utilities.ChangeDateFormat(message.endDate);
                        var dateList = utilities.GenerateDateList(stDate, enDate, message.timePeriod);
                        var temp = new List<EnergyMeterModel>();
                        if (Buildings.Count > 0)
                        {
                            temp = await ReturnPrecinctEnergyDataAsync(dateList, 0, $"{message.timePeriod} Max");
                        }
                        else
                        {
                            temp = await ReturnPrecinctEnergyDataAsync(dateList, precinctEnergyMeter.meterid, $"{message.timePeriod} Max");
                        }
                        var infoModelList = GenerateInformationList(temp);
                        var response = JsonConvert.SerializeObject(infoModelList);
                        return response;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var infoModelList = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, message.DisplayType, message.startDate, message.endDate, message.timePeriod);
                        var tempMess = JsonConvert.SerializeObject(infoModelList);
                        return tempMess;
                    }
                }
                else if (message.MessageType == "Total")
                {
                    if (message.DisplayType == "Individual")
                    {
                        string stDate = utilities.ChangeDateFormat(message.startDate);
                        string enDate = utilities.ChangeDateFormat(message.endDate);
                        var dateList = utilities.GenerateDateList(stDate, enDate, message.timePeriod);
                        var temp = new List<EnergyMeterModel>();
                        if (Buildings.Count > 0)
                        {
                            temp = await ReturnPrecinctEnergyDataAsync(dateList, 0, $"{message.timePeriod} Total");
                        }
                        else
                        {
                            temp = await ReturnPrecinctEnergyDataAsync(dateList, precinctEnergyMeter.meterid, $"{message.timePeriod} Total");
                        }
                        var infoModelList = GenerateInformationList(temp);
                        var response = JsonConvert.SerializeObject(infoModelList);
                        return response;
                    }
                    else if (message.DisplayType == "Collective")
                    {
                        var infoModelList = await ReturnChildDTEnergyDataAsync(message.MessageType, message.DTDetailLevel, message.DisplayType, message.startDate, message.endDate, message.timePeriod);
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
                if (message.MessageType == "LatestTimeStamp")
                {
                    MessageModel tempMes = new MessageModel { DataType = "Operations", MessageType = "LatestTimeStamp" };
                    var mes = JsonConvert.SerializeObject(tempMes);
                    string dayDate = "";
                    if (Buildings.Count > 0)
                    {
                        var response = await myClient.sendMessageAsync(mes, Buildings[0].IP_Address, Buildings[0].Port);
                        dayDate = utilities.DecodeTimestamp(response, "Day");
                    }
                    else
                    {
                        dayDate = utilities.DecodeTimestamp(precinctEnergyMeter.latest_timestamp, "Day");
                    }
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
