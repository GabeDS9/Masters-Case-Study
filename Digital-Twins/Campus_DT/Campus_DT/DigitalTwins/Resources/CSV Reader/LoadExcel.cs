using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class LoadExcel
{
    public List<EnergyMeterData> energyMeterList = new List<EnergyMeterData>();
    public List<OccupancyMeterData> occupancyMeterList = new List<OccupancyMeterData>();
    public List<SolarMeterData> solarMeterList = new List<SolarMeterData>();
    public List<Building_DT.Building> buildingList = new List<Building_DT.Building>();
    public List<Precinct_DT.Precinct> precinctList = new List<Precinct_DT.Precinct>();

    List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

    private string DTConfigurationFile = "DTArchitectureConfiguration.csv";

    #region EnergyMeters
    public List<EnergyMeterData> LoadEnergyMeterData(string buildRecName)
    {
        energyMeterList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Reticulation"].ToString() == buildRecName) || (data[i]["Building"].ToString() == buildRecName) && (data[i]["Meter Type"].ToString() == "Energy"))
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
    public List<Building_DT.Building> LoadBuildingData(string precinct_name)
    {
        buildingList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Precinct"].ToString() == precinct_name) && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-"))
            {
                string name = data[i]["Building"].ToString();
                string latitude = data[i][" Latitude"].ToString();
                string longitude = data[i][" Longitude"].ToString();
                int port = int.Parse(data[i]["Port"].ToString(), System.Globalization.NumberStyles.Integer); ;

                AddBuilding(name, latitude, longitude, port);
            }
        }

        return buildingList;
    }

    void AddBuilding(string name, string latitude, string longitude, int port)
    {
        Building_DT.Building tempBuilding = new Building_DT.Building(name, latitude, longitude, port);

        buildingList.Add(tempBuilding);
    }
    #endregion

    #region Precincts
    public List<Precinct_DT.Precinct> LoadPrecinctData(string campus_name)
    {
        precinctList.Clear();

        string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DTConfigurationFile);

        List<Dictionary<string, object>> data = CSVReader.Read(filepath);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() == campus_name) && (data[i]["Building"].ToString() == "-") && (data[i]["Reticulation"].ToString() == "-"))
            {
                string name = data[i]["Precinct"].ToString();
                string latitude = data[i][" Latitude"].ToString();
                string longitude = data[i][" Longitude"].ToString();

                AddPrecinct(name, latitude, longitude);
            }
        }

        return precinctList;
    }

    void AddPrecinct(string name, string latitude, string longitude)
    {
        Precinct_DT.Precinct tempPrecinct = new Precinct_DT.Precinct(name, latitude, longitude);

        precinctList.Add(tempPrecinct);
    }
    #endregion
}
