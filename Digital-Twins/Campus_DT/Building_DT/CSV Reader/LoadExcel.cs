using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Building_DT
{
    public class LoadExcel
    {
        public List<EnergyMeterData> energyMeterList = new List<EnergyMeterData>();
        public List<OccupancyMeterData> occupancyMeterList = new List<OccupancyMeterData>();
        public List<SolarMeterData> solarMeterList = new List<SolarMeterData>();

        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

        #region EnergyMeters
        public List<EnergyMeterData> LoadEnergyMeterData()
        {
            energyMeterList.Clear();
            
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EnergyMeterNames.csv");

            List <Dictionary<string, object>> data = CSVReader.Read(filepath);

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

            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OccupancyMeterNames.csv");
            List<Dictionary<string, object>> data = CSVReader.Read(filepath);

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

            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SolarMeterNames.csv");
            List<Dictionary<string, object>> data = CSVReader.Read(filepath);

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
    }
}
