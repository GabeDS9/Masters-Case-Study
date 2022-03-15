namespace Mapbox.Examples
{
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.Utilities;
	using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;

    public class SpawnOnMap : MonoBehaviour
	{
		public AbstractMap Map;

		[Geocode]
		string[] _locationStrings;
		Vector2d[] _locations;

		public List<GameObject> MarkerPrefab;
		List<GameObject> _spawnedEnergyObjects;
		List<GameObject> _spawnedWaterObjects;

		List<EnergyMeterData> _energyMeterList;
		List<WaterMeterList> _waterMeterList;

		private List<EnergyMeterData> energyMeterData = new List<EnergyMeterData>();
		private List<WaterMeterData> waterMeterData = new List<WaterMeterData>();

		private void Update()
		{

		}

		public void PopulateEnergyObjects(int meterid, EnergyAPIScript energyManager)
        {
			EnergyMeterData tempEnergyMeter = energyManager.ReturnEnergyMeterData(meterid);

			var locationString = tempEnergyMeter.latitude + ", " + tempEnergyMeter.longitude;
			var instance = Instantiate(MarkerPrefab[0]);
			instance.transform.localPosition = Map.GeoToWorldPosition(Conversions.StringToLatLon(locationString), true);

				float currUse = (float)Math.Abs(tempEnergyMeter.data[tempEnergyMeter.data.Count - 1].ptot_kw);
				instance.transform.localScale = new Vector3(1, currUse, 1);
				instance.transform.position = new Vector3(instance.transform.localPosition.x, currUse / 2, instance.transform.localPosition.z);
		}

		public async void PopulateCurrentEnergyObjects(List<EnergyMeterData> mList)
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
				}*/
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

		/*private async Task<EnergyData> DisplayEnergyAsync(String meterID)
        {
			EnergyData tempEnergy = await Task.Run(() => EnergyAPIScript.GetCurrentMeterData(meterID));
			return tempEnergy;
		}*/

		private Task<EnergyMeterData> GetCurrentEnergyParallelAsync(EnergyMeterData record)
		{
			EnergyMeterData energyData = EnergyAPIScript.GetCurrentMeterData(record.meterid);
			//Debug.Log("Getting energy meter at: " + record.meterid + " - Coord: " + record.latitude + ", " + record.longitude);
			return Task.FromResult(energyData);
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