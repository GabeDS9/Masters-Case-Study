using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadExcel
{
    List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
    private string ConfigurationFile = "DTArchitectureConfiguration";

    public List<ElementModel> LoadBuildings()
    {
        List<ElementModel> buildingList = new List<ElementModel>();
        List<Dictionary<string, object>> data = CSVReader.Read(ConfigurationFile);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() != "-") && (data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() != "-") && (data[i]["Meter Name"].ToString() == "-"))
            {
                string name = data[i]["Building"].ToString();
                string latitude = data[i]["Latitude"].ToString();
                string longitude = data[i]["Longitude"].ToString();

                var childList = new List<string>();
                for (var j = 0; j < data.Count; j++)
                {
                    if ((data[j]["Campus"].ToString() != "-") && (data[j]["Precinct"].ToString() != "-") && (data[j]["Reticulation"].ToString() == "-") && (data[j]["Building"].ToString() == name) && (data[j]["Meter Name"].ToString() != "-"))
                    {
                        string meterID = data[j]["meter_id"].ToString();
                        childList.Add(meterID);
                    }
                }
                if(childList.Count == 0)
                {
                    for (var j = 0; j < data.Count; j++)
                    {
                        if ((data[j]["Campus"].ToString() != "-") && (data[j]["Precinct"].ToString() != "-") && (data[j]["Reticulation"].ToString() == "-") && (data[j]["Building"].ToString() == name) && (data[j]["Meter Name"].ToString() == "-"))
                        {
                            string meterID = data[j]["meter_id"].ToString();
                            childList.Add(meterID);
                        }
                    }
                }
                var tempBuilding = new ElementModel { ElementName = name, ElementType = "Building", Latitude = latitude, Longitude = longitude, ChildElements = childList};
                buildingList.Add(tempBuilding);
            }
        }

        return buildingList;
    }
    public List<ElementModel> LoadPrecincts()
    {
        List<ElementModel> precinctList = new List<ElementModel>();
        List<Dictionary<string, object>> data = CSVReader.Read(ConfigurationFile);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() != "-") && (data[i]["Precinct"].ToString() != "-") && (data[i]["Reticulation"].ToString() == "-") && (data[i]["Building"].ToString() == "-"))
            {
                string name = data[i]["Precinct"].ToString();
                string latitude = data[i]["Latitude"].ToString();
                string longitude = data[i]["Longitude"].ToString();

                var childList = new List<string>();
                for (var j = 0; j < data.Count; j++)
                {
                    if ((data[j]["Campus"].ToString() != "-") && (data[j]["Precinct"].ToString() == name) && ((data[j]["Reticulation"].ToString() != "-") || (data[j]["Building"].ToString() != "-")) && (data[j]["Meter Name"].ToString() == "-"))
                    {
                        string building = data[j]["Building"].ToString();
                        childList.Add(building);
                    }
                }
                var tempPrecinct = new ElementModel { ElementName = name, ElementType = "Precinct", Latitude = latitude, Longitude = longitude, ChildElements = childList };
                precinctList.Add(tempPrecinct);
            }
        }

        return precinctList;
    }
    public List<ElementModel> LoadCampus()
    {
        List<ElementModel> campusList = new List<ElementModel>();
        List<Dictionary<string, object>> data = CSVReader.Read(ConfigurationFile);

        for (var i = 0; i < data.Count; i++)
        {
            if ((data[i]["Campus"].ToString() != "-") && (data[i]["Precinct"].ToString() == "-"))
            {
                string name = data[i]["Campus"].ToString();
                string latitude = data[i]["Latitude"].ToString();
                string longitude = data[i]["Longitude"].ToString();

                var childList = new List<string>();
                for (var j = 0; j < data.Count; j++)
                {
                    if ((data[j]["Campus"].ToString() == name) && (data[j]["Precinct"].ToString() != "-") && (data[j]["Reticulation"].ToString() == "-") && (data[j]["Building"].ToString() == "-"))
                    {
                        string precinct = data[j]["Precinct"].ToString();
                        childList.Add(precinct);
                    }
                }
                var tempCampus = new ElementModel { ElementName = name, ElementType = "Campus", Latitude = latitude, Longitude = longitude, ChildElements = childList };
                campusList.Add(tempCampus);
            }
        }
        return campusList;
    }
    /*
    #region EnergyMeters
    public List<EnergyMeterData> LoadEnergyMeterData()
    {
        energyMeterList.Clear();

        List<Dictionary<string, object>> data = CSVReader.Read("EnergyMeterNames");

        for (var i = 0; i < data.Count; i++)
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
    public List<OccupancyMeterData> LoadOccupancyMeterData()
    {
        occupancyMeterList.Clear();

        List<Dictionary<string, object>> data = CSVReader.Read("OccupancyMeterNames");

        for (var i = 0; i < data.Count; i++)
        {
            int id = int.Parse(data[i][" meter_id"].ToString(), System.Globalization.NumberStyles.Integer);
            string description = data[i][" Meter_Description"].ToString();
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
    public List<SolarMeterData> LoadSolarMeterData()
    {
        solarMeterList.Clear();

        List<Dictionary<string, object>> data = CSVReader.Read("SolarMeterNames");

        for (var i = 0; i < data.Count; i++)
        {
            int id = int.Parse(data[i][" meter_id"].ToString(), System.Globalization.NumberStyles.Integer);
            string description = data[i][" Meter_Description"].ToString();
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
    */
}
