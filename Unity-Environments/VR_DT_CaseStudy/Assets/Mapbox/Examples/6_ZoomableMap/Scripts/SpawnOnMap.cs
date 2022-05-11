namespace Mapbox.Examples
{
    using UnityEngine;
    using UnityEngine.UI;
    using Mapbox.Utils;
    using Mapbox.Unity.Map;
    using Mapbox.Unity.MeshGeneration.Factories;
    using Mapbox.Unity.Utilities;
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class SpawnOnMap : MonoBehaviour
    {
        public AbstractMap Map;

        [Geocode]
        string[] _locationStrings;
        Vector2d[] _locations;

        public List<GameObject> MarkerPrefabs;
        public GameObject VisualInfo;

        private List<VisualisationModel> myVisuals = new List<VisualisationModel>();

        public void PopulateData(string message)
        {
            ClearVisualisationObjects();
            var DataList = DecodeMessage(message);

            foreach(var data in DataList)
            {
                string latitude = data.Latitude;
                string longitude = data.Longitude;

                Vector2d[] locations = new Vector2d[1];

                var locationString = latitude + "," + longitude;
                locations[0] = Conversions.StringToLatLon(locationString);
                int markerPos = GetMarkerType(data.DT_Type, data.DataType);
                var instance = Instantiate(MarkerPrefabs[markerPos]);
                instance.transform.localPosition = Map.GeoToWorldPosition(locations[0], true);

                var infoInstance = Instantiate(VisualInfo);
                var infoText = infoInstance.GetComponentInChildren<Text>();
                infoText.text = $"{data.DT_name}\n{data.Timestamp}\n{data.Value} kW";

                float floatEnergy = (float)Math.Abs(data.Value);
                float adjustedScalePos = floatEnergy / 5;
                instance.transform.localScale = new Vector3(1, adjustedScalePos, 1);
                instance.transform.position = new Vector3(instance.transform.localPosition.x, adjustedScalePos / 2, instance.transform.localPosition.z);
                infoInstance.transform.position = new Vector3(instance.transform.position.x - 1, (adjustedScalePos / 2) + 2, instance.transform.position.z); ;

                VisualisationModel tempModel = new VisualisationModel { Visual = instance, VisualInfo = infoInstance, Data = data };
                myVisuals.Add(tempModel);
            }
        }

        private List<DataModel> DecodeMessage(string message)
        {
            List<DataModel> dataList = new List<DataModel>();
            dataList = JsonConvert.DeserializeObject<List<DataModel>>(message);         
            return dataList;
        }
        private int GetMarkerType(string DT_Type, string DataType)
        {
            int markerPos = 0;
            if(DataType == "Energy")
            {
                if(DT_Type == "Campus")
                {
                    markerPos = 0;
                }
                else if (DT_Type == "Precinct")
                {
                    markerPos = 1;
                }
                else if (DT_Type == "Building")
                {
                    markerPos = 2;
                }
            }
            return markerPos;
        }
        public void ClearVisualisationObjects()
        {
            foreach (var item in myVisuals)
            {
                Destroy(item.Visual);
                Destroy(item.VisualInfo);
            }
            myVisuals.Clear();
        }


        /*public void PopulateEnergyObject(EnergyMeterData energyMeter, List<EnergyAverage> averages, string date, string visualisation_type)
        {
            ClearEnergyObjects(energyMeter.meterid, visualisation_type, legendManager);

            String latitude = energyMeter.latitude;
            String longitude = energyMeter.longitude;
            double energy = 0;

            foreach (var record in averages)
            {
                if (record.timestamp == date)
                {
                    energy = record.ptot_kw;
                    break;
                }
            }

            Vector2d[] locations = new Vector2d[1];

            var locationString = latitude + "," + longitude;
            locations[0] = Conversions.StringToLatLon(locationString);
            var instance = Instantiate(MarkerPrefab[0]);
            instance.transform.localPosition = Map.GeoToWorldPosition(locations[0], true);
            //Debug.Log("Object instantiated for meter " + energyMeter.meterid);

            float floatEnergy = (float)Math.Abs(energy);
            instance.transform.localScale = new Vector3(1, floatEnergy, 1);
            instance.transform.position = new Vector3(instance.transform.localPosition.x, floatEnergy / 2, instance.transform.localPosition.z);
            legendManager.ManageEnergyObjects(energyMeter, visualisation_type, instance, "add");
        }

        public void ClearEnergyObjects(int meterid, string visualisation_type, LegendManager legendManager)
        {
            if (legendManager.spawnedEnergyObjects != null)
            {
                foreach (var item in legendManager.spawnedEnergyObjects)
                {
                    if((item.meterid == meterid) && (item.visualisation_type == visualisation_type))
                    {
                        Destroy(item.visual);
                    }
                }
            }
        }*/

        /*public async Task PopulateEnergyObjectAsync(int meterid, EnergyAPIScript energyManager, String date)
        {
            List<EnergyData> tempEnergyData = await energyManager.GetMeterDataAsync("2022-03-25", "2022-03-28", meterid);
            String latitude = "", longitude = "";

            foreach (var item in energyManager.EnergyMeters)
            {
                if (item.meterid == meterid)
                {
                    latitude = item.latitude;
                    longitude = item.longitude;
                    break;
                }
            }

            int tempYear, tempMonth, tempDay;
            int year, month, day;
            (year, month, day) = energyManager.GetDate(date);
            //Debug.Log($"Requested date: {year}-{month}-{day}");
            double energy = 0;

            foreach (var record in tempEnergyData)
            {
                (tempYear, tempMonth, tempDay) = energyManager.GetDate(record.timestamp);
                //Debug.Log($"Record timestamp: {tempYear}-{tempMonth}-{tempDay}");

                if ((year == tempYear) && (month == tempMonth) && (day == tempDay))
                {
                    energy = record.ptot_kw;
                    //Debug.Log($"{tempEnergyMeter.meterid} timestamp was: {record.timestamp} with energy {energy}");
                    break;
                }
            }

            Vector2d[] locations = new Vector2d[1];

            var locationString = latitude + "," + longitude;
            locations[0] = Conversions.StringToLatLon(locationString);
            var instance = Instantiate(MarkerPrefab[0]);
            instance.transform.localPosition = Map.GeoToWorldPosition(locations[0], true);
            Debug.Log("Object instantiated for meter " + meterid);

            if (tempEnergyData != null)
            {
                float currUse = (float)Math.Abs(energy);
                instance.transform.localScale = new Vector3(1, currUse, 1);
                instance.transform.position = new Vector3(instance.transform.localPosition.x, currUse / 2, instance.transform.localPosition.z);
                //_spawnedEnergyObjects.Add(instance);
            }
        }*/

        /*public async void PopulateCurrentEnergyObjects(List<EnergyMeterData> mList)
        {
            _energyMeterList = mList;
            _locations = new Vector2d[_energyMeterList.Count];
            _spawnedEnergyObjects = new List<GameObject>();
            int i = 0;

            List<Task<EnergyMeterData>> tasks = new List<Task<EnergyMeterData>>();

            foreach (var record in _energyMeterList)
            {

                tasks.Add(Task.Run(() => GetCurrentEnergyParallelAsync(record)));
                i++;

                /*if (i < _energyMeterList.Count)
                {
                    var locationString = record.latitude + ", " + record.longitude;
                    _locations[i] = Conversions.StringToLatLon(locationString);
                    var instance = Instantiate(MarkerPrefab[0]);
                    instance.transform.localPosition = Map.GeoToWorldPosition(_locations[i], true);
                    EnergyMeterData energyData = EnergyAPIScript.GetCurrentMeterData(record.meterid.ToString());
                    //EnergyData energyData = await DisplayEnergyAsync(record.meterid.ToString());
                    if (energyData != null)
                    {
                        float currUse = (float)Math.Abs(energyData.data[energyData.data.Count-1].ptot_kw);
                        instance.transform.localScale = new Vector3(1, currUse, 1);
                        instance.transform.position = new Vector3(instance.transform.localPosition.x, currUse / 2, instance.transform.localPosition.z);
                        _spawnedEnergyObjects.Add(instance);
                        i++;
                    }

                    i++;
                }
            }

            var results = await Task.WhenAll(tasks);

            foreach(var item in results)
            {
                var locationString = item.latitude + ", " + item.longitude;
                Debug.Log(item.meterid + " - " + locationString);
                var instance = Instantiate(MarkerPrefab[0]);
                instance.transform.position = Map.GeoToWorldPosition(Conversions.StringToLatLon(locationString), true);
                if (item.data != null)
                {
                    float currUse = (float)Math.Abs(item.data[item.data.Count - 1].ptot_kw);
                    instance.transform.localScale = new Vector3(1, currUse, 1);
                    instance.transform.position = new Vector3(instance.transform.position.x, currUse / 2, instance.transform.position.z);
                    _spawnedEnergyObjects.Add(instance);
                }
            }
        }

        public void PopulateCurrentWaterObjects(List<WaterMeterList> mList)
        {
            _waterMeterList = mList;
            _locations = new Vector2d[_waterMeterList.Count];
            _spawnedWaterObjects = new List<GameObject>();
            int i = 0;

            foreach (var record in _waterMeterList)
            {
                if (i < _waterMeterList.Count)
                {
                    var locationString = record.latitude + ", " + record.longitude;
                    _locations[i] = Conversions.StringToLatLon(locationString);
                    var instance = Instantiate(MarkerPrefab[1]);
                    instance.transform.localPosition = Map.GeoToWorldPosition(_locations[i], true);
                    WaterData waterData = WaterAPIScript.GetCurrentMeterData(record.meterid.ToString());
                    if (waterData != null)
                    {
                        float currUse = (float)Math.Abs(waterData.ptot_kw);
                        instance.transform.localScale = new Vector3(1, currUse, 1);
                        instance.transform.position = new Vector3(instance.transform.localPosition.x, currUse / 2, instance.transform.localPosition.z);
                        _spawnedWaterObjects.Add(instance);
                        i++;
                    }
                }
            }
        }*/
    }
}