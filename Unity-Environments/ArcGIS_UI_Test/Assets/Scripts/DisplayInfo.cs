using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class DisplayInfo : MonoBehaviour
{
    private List<EnergyMeterList> energyMeterList = new List<EnergyMeterList>();
    private List<WaterMeterList> waterMeterList = new List<WaterMeterList>();
    public Mapbox.Examples.SpawnOnMap spawnOnMap;

    public void PopulateMeters()
    {
        energyMeterList = EnergyAPIScript.GetEnergyMeterList();
        waterMeterList = WaterAPIScript.GetWaterMeterList();
        //spawnOnMap.PopulateMeters();
    }
    public void PopulateEnergyMeters()
    {
        if (energyMeterList != null)
        {
            spawnOnMap.PopulateCurrentEnergyObjects(energyMeterList);
        }
    }

    public void PopulateWaterMeters()
    {
        Debug.Log("Populating water meters");
        if (waterMeterList != null)
        {
            spawnOnMap.PopulateCurrentWaterObjects(waterMeterList);
        }
    }
}
