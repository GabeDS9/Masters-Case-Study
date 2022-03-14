using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayInfo : MonoBehaviour
{
    /*[SerializeField]
    private GameObject cubeVis;
    [SerializeField]
    Mapbox.Unity.Map.AbstractMap map;
    /*[SerializeField]
    private TMP_Dropdown dropDown;*/

    private List<EnergyMeterList> energyMeterList = new List<EnergyMeterList>();
    private List<WaterMeterList> waterMeterList = new List<WaterMeterList>();
    public Mapbox.Examples.SpawnOnMap spawnOnMap;
    private bool isPopulated = false;

    public void PopulateMeters()
    {
        Debug.Log("Getting energy meter list");
        energyMeterList = EnergyAPIScript.GetEnergyMeterList();

        Debug.Log("Getting water meter list");
        waterMeterList = WaterAPIScript.GetWaterMeterList();

        Debug.Log("Meter list populated");
    }
    public void PopulateEnergyMeters()
    {
        Debug.Log("Populating energy meters");
        if (energyMeterList != null)
        {
            spawnOnMap.PopulateEnergyObjects(energyMeterList);
        }
    }

    public void PopulateWaterMeters()
    {
        Debug.Log("Populating water meters");
        if (waterMeterList != null)
        {
            spawnOnMap.PopulateWaterObjects(waterMeterList);
        }
    }
}
