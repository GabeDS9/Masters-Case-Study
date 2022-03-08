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

		public GameObject MarkerPrefab;

		List<MeterData> _meterData;
		List<GameObject> _spawnedObjects;
		List<MeterList> _meterList;		

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
						spawnedObject.transform.localScale = new Vector3(1, (float)_meterData[i].data[_meterData[i].data.Count - 1].ptot_kw / 2, 1);
					}
				}
			}
			
		}

		public void PopulateObjects(List<MeterList> mList)
        {
			_meterList = mList;
			_locations = new Vector2d[_meterList.Count];
			_spawnedObjects = new List<GameObject>();
			_meterData = new List<MeterData>();
			int i = 0;			

		foreach (var record in _meterList)
		{
			var locationString = record.latitude + ", " + record.longitude;
			Debug.Log(locationString);
			_locations[i] = Conversions.StringToLatLon(locationString);
			var instance = Instantiate(MarkerPrefab);
			Debug.Log(_locations[i]);
			instance.transform.localPosition = Map.GeoToWorldPosition(_locations[i], true);
			Debug.Log("Local instance pos: " + instance.transform.localPosition);
			/*MeterData meterData = GetMeterData(record.meterid.ToString());
			Debug.Log("MeterData: " + meterData.description);
			_meterData.Add(meterData);*/
			_meterData.Add(GetMeterData(record.meterid.ToString()));
			instance.transform.localScale = new Vector3(1, (float)Math.Abs(_meterData[i].data[_meterData[i].data.Count - 1].ptot_kw / 4), 1);
			instance.transform.localPosition = new Vector3(instance.transform.localPosition.x, (float) Math.Abs(_meterData[i].data[_meterData[i].data.Count - 1].ptot_kw / 8), instance.transform.localPosition.z);
			_spawnedObjects.Add(instance);
			i++;
		}
		}

		private MeterData GetMeterData(string meterID)
        {
			string fromDate = "2022-03-07%2009:25:00";
			string currDate = "2022-03-07%2009:35:00";
			string interval = "ts_5min";
			return EnergyAPIScript.GetMeterData(fromDate, currDate, meterID, interval);
        }
	}
}