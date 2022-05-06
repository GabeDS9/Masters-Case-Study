using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;

namespace Campus_DT
{
    class CampusManager
    {
        private string startingDate = "2022-05-01 00:00:00";
        private Campus campus;
        private Stopwatch stopWatch = new Stopwatch();
        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        
        public void InitialiseCampus()
        {
            LoadExcel excel = new LoadExcel();
            string Campus_name = "Stellenbosch";

            var camp = excel.LoadCampusData(Campus_name, startingDate);

            campus = camp[0];

            stopWatch.Start();

            while (true)
            {
                double ts = stopWatch.Elapsed.TotalSeconds;
                if (ts >= 60)
                {
                    campus.GetUpdatedData();
                    stopWatch.Restart();
                }
            }

            //db = new CampusDBDataAccess(Campus_name);
            //var campus = new CampusModel(Campus_name, Latitude, Longitude, Precincts);
            //db.CreateCampus(campus);

            /*foreach (var item in Precincts)
            {
                Console.WriteLine("Campus: " + Campus_name);
                
                Console.WriteLine("Precinct: " + item.Precinct_name);

                foreach(var record in item.Buildings)
                {
                    Console.WriteLine(item.Precinct_name + " - Building: " + record.Building_name);
                    Console.WriteLine(record.Building_name + " energy meters");
                    foreach (var rec in record.EnergyMeters)
                    {
                        Console.WriteLine(record.Building_name + " - " + rec.description);
                    }
                    /*Console.WriteLine(record.Building_name + " occupancy meters");
                    foreach (var rec in record.OccupancyMeters)
                    {
                        Console.WriteLine(record.Building_name + " - " + rec.description);
                    }
                    Console.WriteLine(record.Building_name + " solar meters");
                    foreach (var rec in record.SolarMeters)
                    {
                        Console.WriteLine(record.Building_name + " - " + rec.description);
                    }
                }

                /*foreach (var record in item.SolarMeters)
                {
                    Console.WriteLine(item.Precinct_name + " - " + record.Reticulation_name);
                }

                foreach (var record in item.EnergyMeters)
                {
                    Console.WriteLine(item.Precinct_name + " - " + record.Reticulation_name);
                }
            }*/
        }
    }
}
