using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Building_DT
{    class Building
    {
        // Intialise building specific info
        public string Name { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public List<EnergyMeterData> EnergyMeters { get; set; }
        public List<OccupancyMeterData> OccupancyMeters { get; set; }
        public List<SolarMeterData> SolarMeters { get; set; }

        private EnergyMeters energyManager = new EnergyMeters();
        private OccupancyMeters occupancyManager = new OccupancyMeters();
        private SolarMeters solarManager = new SolarMeters();

        public Building(string name, int latitude, int longitude, List<EnergyMeterData> energyMeterList)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            EnergyMeters = energyMeterList;
        }

        // Get available energy meters in the building
        public void GetMeterLists()
        {
            EnergyMeters = energyManager.LoadEnergyMeterList();
            OccupancyMeters = occupancyManager.LoadOccupancyMeterList();
            SolarMeters = solarManager.LoadSolarMeterList();
        }

        public void SendMeterList(SocketServer server)
        {
            foreach(var record in EnergyMeters)
            {
                server.SendMessage(record.description);
                string temp = server.ReceiveMessage();

                Console.WriteLine(temp);
            }

            if(server.ReceiveMessage() == "<EOF>")
            {
                server.SendMessage("<EOF>");
                server.CloseServer();
            }
            
        }
    }
}
