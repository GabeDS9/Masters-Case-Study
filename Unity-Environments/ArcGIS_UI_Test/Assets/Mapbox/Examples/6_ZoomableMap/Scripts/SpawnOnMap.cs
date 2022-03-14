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

		public List<GameObject> MarkerPrefab;
		List<GameObject> _spawnedEnergyObjects;
		List<GameObject> _spawnedWaterObjects;

		List<EnergyMeterList> _energyMeterList;
		List<WaterMeterList> _waterMeterList;

		private List<EnergyMeterData> energyMeterData = new List<EnergyMeterData>();
		private List<WaterMeterData> waterMeterData = new List<WaterMeterData>();

		private string fromDate, toDate = "";

        private void Update()
		{

		}

		public void PopulateMeters()
        {
			(toDate, fromDate) = APICaller.GetCurrentDateTime();
			fromDate = "2022-03-01%2000:00:00";

			foreach (var record in _energyMeterList)
            {
				energyMeterData.Add(EnergyAPIScript.GetMeterData(fromDate, toDate, record.meterid.ToString()));
			}
			foreach (var record in _waterMeterList)
			{
				waterMeterData.Add(WaterAPIScript.GetMeterData(fromDate, toDate, record.meterid.ToString()));
			}

		}

		public void PopulateCurrentEnergyObjects(List<EnergyMeterList> mList)
        {
			_energyMeterList = mList;
			_locations = new Vector2d[_energyMeterList.Count];
			_spawnedEnergyObjects = new List<GameObject>();
			int i = 0;			

			foreach (var record in _energyMeterList)
			{
				if (i < _energyMeterList.Count)
				{
					var locationString = record.latitude + ", " + record.longitude;
					_locations[i] = Conversions.StringToLatLon(locationString);
					var instance = Instantiate(MarkerPrefab[0]);
					instance.transform.localPosition = Map.GeoToWorldPosition(_locations[i], true);
					EnergyData energyData = EnergyAPIScript.GetCurrentMeterData(record.meterid.ToString());
					if (energyData != null)
					{
							float currUse = (float)Math.Abs(energyData.ptot_kw);
							instance.transform.localScale = new Vector3(1, currUse, 1);
							instance.transform.position = new Vector3(instance.transform.localPosition.x, currUse / 2, instance.transform.localPosition.z);
							_spawnedEnergyObjects.Add(instance);
							i++;
					}
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
		}
	}
}