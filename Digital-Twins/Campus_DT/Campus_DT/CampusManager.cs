using System;
using System.Collections.Generic;

namespace Campus_DT
{
    class CampusManager
    {
        public static string Campus_name { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public static List<Precinct_DT.Precinct> Precincts { get; set; }

        private static Precinct_DT.PrecinctManager precinctManager = new Precinct_DT.PrecinctManager();

        public static void Main(string[] args)
        {
            Campus_name = "Stellenbosch";
            InitialiseCampus();
            
            foreach(var item in Precincts)
            {
                Console.WriteLine("Precinct: " + item.Precinct_name);

                foreach(var record in item.Buildings)
                {
                    Console.WriteLine(item.Precinct_name + " - Building: " + record.Building_name);
                    Console.WriteLine(record.Building_name + " energy meters");
                    foreach (var rec in record.EnergyMeters)
                    {
                        Console.WriteLine(record.Building_name + " - " + rec.description);
                    }
                    Console.WriteLine(record.Building_name + " occupancy meters");
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
                }*/
            }
        }

        private static void InitialiseCampus()
        {
            Precincts = precinctManager.InitialisePrecincts(Campus_name);
        }
    }
}
