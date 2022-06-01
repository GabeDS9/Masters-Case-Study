using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Bootstrap
{
    private static LoadExcel excel = new LoadExcel();
    private static List<Building_DT.Building> buildingList = new List<Building_DT.Building>();
    private static List<Precinct_DT.Precinct> precinctList = new List<Precinct_DT.Precinct>();
    private static List<Campus_DT.Campus> campusList = new List<Campus_DT.Campus>();
    public static void Main(String[] args)
    {
        InitialiseDigitalTwins();
        /*Thread DTsThread = new Thread(myCampus.InitialiseCampus)
        {
            Name = "DT Thread"
        };

        Thread ServicesThread = new Thread(myServices.InitialiseServices)
        {
            Name = "Services Thread"
        };

        DTsThread.Start();
        ServicesThread.Start();*/
    }

    private static void InitialiseDigitalTwins()
    {
        buildingList = excel.LoadBuildingData();
        foreach(var build in buildingList)
        {
            Thread buildingThread = new Thread(build.InitialiseBuilding);
            buildingThread.Start();
        }
        precinctList = excel.LoadPrecinctData();
        foreach (var prec in precinctList)
        {
            Thread precinctThread = new Thread(prec.InitialisePrecinct);
            precinctThread.Start();
        }
        campusList = excel.LoadCampusData();
        foreach (var camp in campusList)
        {
            Thread campusThread = new Thread(camp.InitialiseCampus);
            campusThread.Start();
        }
    }
}
