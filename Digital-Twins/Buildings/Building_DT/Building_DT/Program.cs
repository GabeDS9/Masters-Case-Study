using System;
using System.Threading;

namespace Building_DT
{    class Program
    {
        static void Main(string[] args)
        {
            /*SocketServer server = new SocketServer();
            server.StartServer();*/

            Building tempBuilding = new Building("Temp Building", 0, 0, null);
            tempBuilding.GetEnergyMeterList();

            /*server.CloseServer();

            while (true)
            {
                if (server.ReceiveMessage() == "MeterList")
                {
                    tempBuilding.SendMeterList(server);
                }
            }

            server.CloseServer();

            /*Console.WriteLine("Starting 5 minute timer at " + DateTime.Now);
            Thread.Sleep(5000);
            Console.WriteLine("Getting current energy usage");

            tempBuilding.GetCurrentMeterReadings();

            Console.WriteLine("Starting 5 minute timer at " + DateTime.Now);
            Thread.Sleep(360000);
            Console.WriteLine("Getting current energy usage");

            tempBuilding.GetCurrentMeterReadings();*/
        }
    }
}
