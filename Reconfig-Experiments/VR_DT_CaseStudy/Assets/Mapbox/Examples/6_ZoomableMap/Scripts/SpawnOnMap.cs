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
        private List<string> dtNames = new List<string>();
        private List<DataModel> DataList = new List<DataModel>();

        public void PopulateData(string message, List<string> DateList, string serviceType)
        {
            ClearVisualisationObjects();
            //DataList.Clear();
            if (DateList != null)
            {
                PopulateTimePeriodData(message, DateList, serviceType);
            }
            else
            {
                PopulateCurrentData(message);
            }
        }
        private void PopulateTimePeriodData(string message, List<string> DateList, string serviceType)
        {
            Debug.Log(message);
            var DataList = DecodeMessage(message);
            foreach (var data in DataList)
            {
                string latitude = data.Latitude;
                string longitude = data.Longitude;

                Vector2d[] locations = new Vector2d[1];

                var locationString = latitude + "," + longitude;
                locations[0] = Conversions.StringToLatLon(locationString);
                int markerPos = GetMarkerType(data.DT_Type, data.DataType);
                var instance = Instantiate(MarkerPrefabs[markerPos]);
                instance.transform.localPosition = Map.GeoToWorldPosition(locations[0], true);

                float floatEnergy = (float)Math.Abs(data.Value);
                float adjustedScalePos = floatEnergy / 10;
                instance.transform.localScale = new Vector3(1, adjustedScalePos, 1);
                instance.transform.position = new Vector3(instance.transform.localPosition.x, adjustedScalePos / 2, instance.transform.localPosition.z);

                var infoInstance = Instantiate(VisualInfo);
                var infoText = infoInstance.GetComponentInChildren<Text>();
                var roundedData = (float)(Math.Round((decimal)data.Value, 3));
                if (serviceType == "Averages")
                {
                    infoText.text = $"{data.DT_name}\n{data.Timestamp}\n{roundedData} kWh";
                }
                else if (serviceType == "Max")
                {
                    infoText.text = $"{data.DT_name}\n{data.Timestamp}\n{roundedData} kW";
                }
                else if (serviceType == "Total")
                {
                    infoText.text = $"{data.DT_name}\n{data.Timestamp}\nTotal: {roundedData} kWh";
                }
                //Debug.Log(infoText.text);
                infoInstance.transform.position = new Vector3(instance.transform.position.x - 1, (adjustedScalePos / 2) + 5, instance.transform.position.z); ;

                VisualisationModel tempModel = new VisualisationModel { Visual = instance, VisualInfo = infoInstance, Data = data, InitialVisualScale = adjustedScalePos, InitialInfoScale = infoInstance.transform.localScale };
                myVisuals.Add(tempModel);
            }

            foreach (var vis in myVisuals)
            {
                if (DateList[0] == vis.Data.Timestamp)
                {
                    vis.Visual.SetActive(true);
                    vis.VisualInfo.SetActive(true);
                }
                else
                {
                    vis.Visual.SetActive(false);
                    vis.VisualInfo.SetActive(false);
                }
            }
        }
        private void PopulateCurrentData(string message)
        {
            //Debug.Log(message);
            var DataList = DecodeMessage(message);
            foreach (var data in DataList)
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
                float adjustedScalePos = floatEnergy / 10;
                adjustedScalePos = (float)(Math.Round((decimal)adjustedScalePos, 3));
                instance.transform.localScale = new Vector3(1, adjustedScalePos, 1);
                instance.transform.position = new Vector3(instance.transform.localPosition.x, adjustedScalePos / 2, instance.transform.localPosition.z);
                infoInstance.transform.position = new Vector3(instance.transform.position.x - 1, (adjustedScalePos / 2) + 4, instance.transform.position.z);

                VisualisationModel tempModel = new VisualisationModel { Visual = instance, VisualInfo = infoInstance, Data = data, InitialVisualScale = adjustedScalePos, InitialInfoScale = infoInstance.transform.localScale };
                myVisuals.Add(tempModel);
            }
        }
        public void ChangeVisualisationDate(string date)
        {
            foreach (var vis in myVisuals)
            {
                if (date == vis.Data.Timestamp)
                {
                    vis.Visual.SetActive(true);
                    vis.VisualInfo.SetActive(true);
                }
                else
                {
                    vis.Visual.SetActive(false);
                    vis.VisualInfo.SetActive(false);
                }
            }
        }
        public void ChangeVisualisationScale(float scale)
        {
            foreach(var vis in myVisuals)
            {
                float newScale = vis.InitialVisualScale * scale;
                vis.Visual.transform.localScale = new Vector3(2*scale, newScale, 2*scale);
                vis.Visual.transform.position = new Vector3(vis.Visual.transform.localPosition.x, newScale / 2, vis.Visual.transform.localPosition.z);
                vis.VisualInfo.transform.localScale = new Vector3(vis.InitialInfoScale.x * (scale+0.2f), vis.InitialInfoScale.y * (scale + 0.2f), vis.InitialInfoScale.z * (scale + 0.2f)); 
                vis.VisualInfo.transform.position = new Vector3(vis.Visual.transform.position.x - 1, (newScale / 2) + (4.5f*scale), vis.Visual.transform.position.z);
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
            if (DataType == "Energy")
            {
                if (DT_Type == "Campus")
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
    }
}