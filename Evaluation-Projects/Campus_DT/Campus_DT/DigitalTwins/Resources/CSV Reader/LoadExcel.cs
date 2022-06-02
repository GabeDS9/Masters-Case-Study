using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Models;

public class LoadExcel
{
    public List<EnergyMeterData> energyMeterList = new List<EnergyMeterData>();
    public List<OccupancyMeterData> occupancyMeterList = new List<OccupancyMeterData>();
    public List<SolarMeterData> solarMeterList = new List<SolarMeterData>();
    public List<Building_DT.Building> buildingList = new List<Building_DT.Building>();
    public List<Precinct_DT.Precinct> precinctList = new List<Precinct_DT.Precinct>();
    public List<Campus_DT.Campus> campusList = new List<Campus_DT.Campus>();

    List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

    private string DTConfigurationFile = "LargeDTArchitectureConfiguration.csv";

    #region EnergyMeters
    public List<EnergyMeterData> LoadEnergyMeterData(string elementName)
    {
        energyMeterList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if (((data[i]["Precinct"].ToString() == elementName) || (data[i]["Reticulation"].ToString() == elementName) || (data[i]["Building"].ToString() == elementName)) && (data[i]["Meter Type"].ToString() == "Energy"))
            {
                int id = int.Parse(data[i]["meter_id"].ToString(), System.Globalization.NumberStyles.Integer);
                string description = data[i]["description"].ToString();
                string make = data[i]["make"].ToString();
                string manufacturer = data[i]["manufacturer"].ToString();
                string type = data[i]["type"].ToString();
                string serialno = data[i]["Serial Number"].ToString();
                string model = data[i]["model"].ToString();
                string yard_number = data[i]["yard_number"].ToString();
                string building_no = data[i]["Building_No"].ToString();
                string floor = data[i]["Floor"].ToString();
                string room_no = data[i]["Room_No"].ToString();
                string latitude = data[i]["Latitude"].ToString();
                string longitude = data[i]["Longitude"].ToString();

                AddEnergyMeter(id, description, make, manufacturer, type, serialno, model, yard_number, building_no, floor, room_no, latitude, longitude);
            }
        }

        return energyMeterList;
    }

    void AddEnergyMeter(int id, string description, string make, string manufacturer, string type, string serialno, string model,
        string yard_number, string building_no, string floor, string room_no, string latitude, string longitude)
    {
        EnergyMeterData tempMeter = new EnergyMeterData();

        tempMeter.meterid = id;
        tempMeter.description = description;
        tempMeter.make = make;
        tempMeter.manufacturer = manufacturer;
        tempMeter.type = type;
        tempMeter.serial_no = serialno;
        tempMeter.model = model;
        tempMeter.yard_no = yard_number;
        tempMeter.building_no = building_no;
        tempMeter.floor = floor;
        tempMeter.room_no = room_no;
        tempMeter.latitude = latitude;
        tempMeter.longitude = longitude;

        energyMeterList.Add(tempMeter);
    }
    #endregion

    #region OccupancyMeters
    public List<OccupancyMeterData> LoadOccupancyMeterData(string buildingName)
    {
        occupancyMeterList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);
        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Building"].ToString() == buildingName) && (data[i]["Meter Type"].ToString() == "Occupancy"))
            {
                int id = int.Parse(data[i][" meter_id"].ToString(), System.Globalization.NumberStyles.Integer);
                string description = data[i][" description"].ToString();
                string make = data[i][" make"].ToString();
                string manufacturer = data[i][" manufacturer"].ToString();
                string type = data[i][" type"].ToString();
                string serialno = data[i][" Serial Number"].ToString();
                string model = data[i][" model"].ToString();
                string yard_number = data[i][" yard_number"].ToString();
                string building_no = data[i][" Building_No"].ToString();
                string floor = data[i][" Floor"].ToString();
                string room_no = data[i][" Room_No"].ToString();
                string latitude = data[i][" Latitude"].ToString();
                string longitude = data[i][" Longitude"].ToString();

                AddOccupancyMeter(id, description, make, manufacturer, type, serialno, model, yard_number, building_no, floor, room_no, latitude, longitude);
            }
        }

        return occupancyMeterList;
    }

    void AddOccupancyMeter(int id, string description, string make, string manufacturer, string type, string serialno, string model,
        string yard_number, string building_no, string floor, string room_no, string latitude, string longitude)
    {
        OccupancyMeterData tempMeter = new OccupancyMeterData();

        tempMeter.meterid = id;
        tempMeter.description = description;
        tempMeter.make = make;
        tempMeter.manufacturer = manufacturer;
        tempMeter.type = type;
        tempMeter.serial_no = serialno;
        tempMeter.model = model;
        tempMeter.yard_no = yard_number;
        tempMeter.building_no = building_no;
        tempMeter.floor = floor;
        tempMeter.room_no = room_no;
        tempMeter.latitude = latitude;
        tempMeter.longitude = longitude;

        occupancyMeterList.Add(tempMeter);
    }
    #endregion

    #region SolarMeters
    public List<SolarMeterData> LoadSolarMeterData(string buildRecName)
    {
        solarMeterList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);
        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Reticulation"].ToString() == buildRecName) || (data[i]["Building"].ToString() == buildRecName) && (data[i]["Meter Type"].ToString() == "Solar"))
            {
                int id = int.Parse(data[i][" meter_id"].ToString(), System.Globalization.NumberStyles.Integer);
                string description = data[i][" description"].ToString();
                string make = data[i][" make"].ToString();
                string manufacturer = data[i][" manufacturer"].ToString();
                string type = data[i][" type"].ToString();
                string serialno = data[i][" Serial Number"].ToString();
                string model = data[i][" model"].ToString();
                string yard_number = data[i][" yard_number"].ToString();
                string building_no = data[i][" Building_No"].ToString();
                string floor = data[i][" Floor"].ToString();
                string room_no = data[i][" Room_No"].ToString();
                string latitude = data[i][" Latitude"].ToString();
                string longitude = data[i][" Longitude"].ToString();

                AddSolarMeter(id, description, make, manufacturer, type, serialno, model, yard_number, building_no, floor, room_no, latitude, longitude);
            }
        }

        return solarMeterList;
    }

    void AddSolarMeter(int id, string description, string make, string manufacturer, string type, string serialno, string model,
        string yard_number, string building_no, string floor, string room_no, string latitude, string longitude)
    {
        SolarMeterData tempMeter = new SolarMeterData();

        tempMeter.meterid = id;
        tempMeter.description = description;
        tempMeter.make = make;
        tempMeter.manufacturer = manufacturer;
        tempMeter.type = type;
        tempMeter.serial_no = serialno;
        tempMeter.model = model;
        tempMeter.yard_no = yard_number;
        tempMeter.building_no = building_no;
        tempMeter.floor = floor;
        tempMeter.room_no = room_no;
        tempMeter.latitude = latitude;
        tempMeter.longitude = longitude;

        solarMeterList.Add(tempMeter);
    }
    #endregion

    #region Buildings
    public List<Building_DT.Building> LoadBuildingData()
    {
        buildingList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);
        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() != "-") && (data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-"))
            {
                string name = data[i]["Building"].ToString();
                string latitude = data[i]["Latitude"].ToString();
                string longitude = data[i]["Longitude"].ToString();
                string ipAdd = data[i]["IP_Address"].ToString();
                int port = int.Parse(data[i]["Port"].ToString(), System.Globalization.NumberStyles.Integer);
                string startingDate = "2022-05-01 00:00:00";
                string location = data[i]["Location"].ToString();

                if (location == "Local")
                {
                    AddBuilding(name, latitude, longitude, ipAdd, port, startingDate);
                }
            }
        }

        return buildingList;
    }

    void AddBuilding(string name, string latitude, string longitude, string ipAdd, int port, string startingDate)
    {
        Building_DT.Building tempBuilding = new Building_DT.Building(name, latitude, longitude, ipAdd, port, startingDate);

        buildingList.Add(tempBuilding);
    }    
    #endregion

    #region Precincts
    public List<Precinct_DT.Precinct> LoadPrecinctData()
    {
        precinctList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() != "-") && (data[i]["Precinct"].ToString() != "-") && (data[i]["Building"].ToString() == "-") && (data[i]["Reticulation"].ToString() == "-"))
            {
                string name = data[i]["Precinct"].ToString();
                string latitude = data[i]["Latitude"].ToString();
                string longitude = data[i]["Longitude"].ToString();
                string ipAdd = data[i]["IP_Address"].ToString();
                int port = int.Parse(data[i]["Port"].ToString(), System.Globalization.NumberStyles.Integer);
                string startingDate = data[i]["Starting date"].ToString();
                string location = data[i]["Location"].ToString();

                if (location == "Local")
                {
                    AddPrecinct(name, latitude, longitude, ipAdd, port, startingDate);
                }
            }
        }

        return precinctList;
    }

    void AddPrecinct(string name, string latitude, string longitude, string ipAdd, int port, string startingDate)
    {
        Precinct_DT.Precinct tempPrecinct = new Precinct_DT.Precinct(name, latitude, longitude, ipAdd, port, startingDate);

        precinctList.Add(tempPrecinct);
    }
    public List<ChildDT> LoadPrecinctChildren(string precinctName)
    {
        List<ChildDT> childList = new List<ChildDT>();
        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);
        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() != "-") && (data[i]["Precinct"].ToString() == precinctName) && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-"))
            {
                string name = data[i]["Building"].ToString();
                string ipAdd = data[i]["IP_Address"].ToString();
                int port = int.Parse(data[i]["Port"].ToString(), System.Globalization.NumberStyles.Integer);

                ChildDT temp = new ChildDT { ChildDT_Name = name, IP_Address = ipAdd, Port = port };
                childList.Add(temp);
            }
        }
        return childList;
    }
    #endregion

    #region Campus
    public List<Campus_DT.Campus> LoadCampusData()
    {
        campusList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() != "-") && (data[i]["Precinct"].ToString() == "-"))
            {
                string name = data[i]["Campus"].ToString();
                string latitude = data[i]["Latitude"].ToString();
                string longitude = data[i]["Longitude"].ToString();
                string ipAdd = data[i]["IP_Address"].ToString();
                int port = int.Parse(data[i]["Port"].ToString(), System.Globalization.NumberStyles.Integer);
                string startingDate = data[i]["Starting date"].ToString();
                string location = data[i]["Location"].ToString();

                if (location == "Local")
                {
                    AddCampus(name, latitude, longitude, ipAdd, port, startingDate);
                }
            }
        }

        return campusList;
    }

    void AddCampus(string name, string latitude, string longitude, string ipAdd, int port, string startingDate)
    {
        Campus_DT.Campus tempCampus = new Campus_DT.Campus(name, latitude, longitude, ipAdd, port, startingDate);

        campusList.Add(tempCampus);
    }
    public List<ChildDT> LoadCampusChildren(string campusName)
    {
        List<ChildDT> childList = new List<ChildDT>();
        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);
        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() == campusName) && (data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() == "-"))
            {
                string name = data[i]["Precinct"].ToString();
                string ipAdd = data[i]["IP_Address"].ToString();
                int port = int.Parse(data[i]["Port"].ToString(), System.Globalization.NumberStyles.Integer);

                ChildDT temp = new ChildDT { ChildDT_Name = name, IP_Address = ipAdd, Port = port };
                childList.Add(temp);
            }
        }
        return childList;
    }
    #endregion
}
