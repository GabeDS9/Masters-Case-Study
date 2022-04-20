using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Building_DT
{
    public class Building
    {
        // Intialise building specific info
        public string Building_name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public List<EnergyMeterData> EnergyMeters { get; set; }
        public List<OccupancyMeterData> OccupancyMeters { get; set; }
        public List<SolarMeterData> SolarMeters { get; set; }

        private EnergyMeters energyManager = new EnergyMeters();
        private OccupancyMeters occupancyManager = new OccupancyMeters();
        private SolarMeters solarManager = new SolarMeters();
        private APICaller apiCaller = new APICaller();
        private Stopwatch stopWatch = new Stopwatch();

        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        public Building(string name, string latitude, string longitude)
        {
            Building_name = name;
            Latitude = latitude;
            Longitude = longitude;
            _ = InitialiseBuildingAsync();
        }

        # region Initialisation Functions
        // Get available energy meters in the building
        public async Task InitialiseBuildingAsync()
        {
            EnergyMeters = energyManager.LoadEnergyMeterList(Building_name);
            OccupancyMeters = occupancyManager.LoadOccupancyMeterList();
            SolarMeters = solarManager.LoadSolarMeterList();
            await Task.Run(() => InitialPopulateDataBase());
            await Task.Run(() => RunBuildingDT());
        }

        private void InitialPopulateDataBase()
        {
            InitialiseMeterData();
            ContextGeneration("2022-04-01 00:00:00", apiCaller.GetCurrentDateTime().Item2);
            SaveToDataBase();
        }


        private void InitialiseMeterData()
        {
            foreach (var item in EnergyMeters)
            {
                item.data = energyManager.GetMeterData("2022-04-18", apiCaller.GetCurrentDateTime().Item2, item.meterid);
            }
        }

        private void InitialContextGeneration(String startDate, String endDate)
        {
            CalculateInitialDayAverage(startDate, endDate);
            Console.WriteLine("Average Energy Reading for " + EnergyMeters[0].description + " was " + EnergyMeters[0].day_average[0].timestamp + " - " + EnergyMeters[0].day_average[0].ptot_kw);
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
        #endregion

        private void SaveToDataBase()
        {

        }

        public void RunBuildingDT()
        {
            stopWatch.Start();
            while (true)
            {
                double ts = stopWatch.Elapsed.TotalSeconds;
                if (ts >= 3)
                {
                    GetCurrentMeterData();
                    ContextGeneration(apiCaller.GetCurrentDateTime().Item1);
                    stopWatch.Restart();
                }
            }
        }
        private float AccessDatabase()
        {
            return 0;
        }

        private void GetCurrentMeterData()
        {
            foreach (var item in EnergyMeters)
            {
                item.data = energyManager.GetCurrentEnergyData(item.meterid);
            }
        }

        #region Services
        private void ContextGeneration(String currDate)
        {
            //CalculateDayAverage(currDate);
            Console.WriteLine("Average Energy Reading for " + EnergyMeters[0].description + " was " + EnergyMeters[0].day_average[0].timestamp + " - " + EnergyMeters[0].day_average[0].ptot_kw);
            //CalculateMonthAverage();
        }
        /*
        public void CalculateDayAverage(string currDate)
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
        }*/

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

