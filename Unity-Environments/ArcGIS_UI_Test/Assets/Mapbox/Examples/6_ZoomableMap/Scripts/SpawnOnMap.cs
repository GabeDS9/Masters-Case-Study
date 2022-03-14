namespace Mapbox.Examples
{
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.Utilities;
	using System.Collections.Generic;
    using System;

    public class SpawnOnMap : MonoBehaviour
	{
		public AbstractMap Map;

		[Geocode]
		string[] _locationStrings;
		Vector2d[] _locations;

		float _spawnScale = 1f;

		public List<GameObject> MarkerPrefab;
		List<GameObject> _spawnedObjects;

		List<EnergyMeterData> _energyMeterData;
		List<EnergyMeterList> _energyMeterList;

		List<WaterMeterData> _waterMeterData;
		List<WaterMeterList> _waterMeterList;

		WaterData waterData;

		private string fromDate;
		private string toDate;


		private void Update()
		{
			if (_spawnedObjects != null)
			{
				int count = _spawnedObjects.Count;
				if (count > 0)
				{
					for (int i = 0; i < count; i++)
					{
						var spawnedObject = _spawnedObjects[i];
						var location = _locations[i];
						spawnedObject.transform.localPosition = Map.GeoToWorldPosition(location, true);
						spawnedObject.transform.localScale = new Vector3(1, (float)_energyMeterData[i].data[_energyMeterData[i].data.Count - 1].ptot_kw / 2, 1);
					}
				}
			}
			
		}

		public void PopulateEnergyObjects(List<EnergyMeterList> mList)
        {
			_energyMeterList = mList;
			_locations = new Vector2d[_energyMeterList.Count];
			_spawnedObjects = new List<GameObject>();
			_energyMeterData = new List<EnergyMeterData>();
			int i = 0;			

		foreach (var record in _energyMeterList)
		{
				if (i < _waterMeterList.Count)
				{
					var locationString = record.latitude + ", " + record.longitude;
					_locations[i] = Conversions.StringToLatLon(locationString);
					var instance = Instantiate(MarkerPrefab[0]);
					instance.transform.localPosition = Map.GeoToWorldPosition(_locations[i], true);
					_energyMeterData.Add(GetEnergyMeterData(record.meterid.ToString()));
					float currUse = (float)Math.Abs(_energyMeterData[i].data[_energyMeterData[i].data.Count - 1].ptot_kw);
					instance.transform.localScale = new Vector3(1, currUse, 1);
					instance.transform.position = new Vector3(instance.transform.localPosition.x, currUse/2, instance.transform.localPosition.z);
					_spawnedObjects.Add(instance);
					i++;
				}
		}
		}

		public void PopulateWaterObjects(List<WaterMeterList> mList)
		{
			_waterMeterList = mList;
			_locations = new Vector2d[_waterMeterList.Count];
			_spawnedObjects = new List<GameObject>();
			_waterMeterData = new List<WaterMeterData>();
			int i = 0;

			foreach (var record in _waterMeterList)
			{
                if (i < _waterMeterList.Count)
                {
					var locationString = record.latitude + ", " + record.longitude;
					_locations[i] = Conversions.StringToLatLon(locationString);
					var instance = Instantiate(MarkerPrefab[1]);
					instance.transform.localPosition = Map.GeoToWorldPosition(_locations[i], true);
					//_waterMeterData.Add(GetWaterMeterData(record.meterid.ToString()));
					//float currUse = (float)Math.Abs(_waterMeterData[i].data[_waterMeterData[i].data.Count - 1].ptot_kw);
					waterData = WaterAPIScript.GetCurrentMeterData(record.meterid.ToString());
					float currUse = (float)Math.Abs(waterData.ptot_kw);
					instance.transform.localScale = new Vector3(1, currUse, 1);
					instance.transform.position = new Vector3(instance.transform.localPosition.x, currUse / 2, instance.transform.localPosition.z);
					_spawnedObjects.Add(instance);
					i++;
				}
			}
		}

		private EnergyMeterData GetEnergyMeterData(String meterID)
        {
			String fromDate = "2022-03-07%2009:25:00";
			string currDate = "2022-03-07%2009:35:00";
			return EnergyAPIScript.GetMeterData(fromDate, currDate, meterID);
        }

		private WaterMeterData GetWaterMeterData(String meterID)
        {
			String fromDate = "2022-03-07%2009:25:00";
			String currDate = "2022-03-07%2009:35:00";
			return WaterAPIScript.GetMeterData(fromDate, currDate, meterID);
		}
	}
}