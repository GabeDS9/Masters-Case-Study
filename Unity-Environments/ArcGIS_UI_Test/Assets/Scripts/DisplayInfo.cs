using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

public class DisplayInfo : MonoBehaviour
{
    private List<EnergyMeterData> energyMeterData = new List<EnergyMeterData>();
    private List<WaterMeterList> waterMeterList = new List<WaterMeterList>();
    public Mapbox.Examples.SpawnOnMap spawnOnMap;
    public EnergyAPIScript energyManager = new EnergyAPIScript();
    public Button energyButton;

    private bool isPopulated = false;

    private void Start()
    {
        energyButton.enabled = false;
    }

    private void Update()
    {
        if (isPopulated)
        {
            energyButton.enabled = true;
        }
        else
        {
            energyButton.enabled = true;
        }
    }
    
    public void PopulateMeters()
    {
        Task.Run(() => energyManager.InitialiseEnergyMetersAsync());
    }
    public void PopulateEnergyMeters()
    {
        spawnOnMap.ClearEnergyObjects();
        foreach(var record in energyManager.EnergyMeters)
        {
            spawnOnMap.PopulateEnergyObject(record.meterid, energyManager, "2022-03-23");
        }
    }

    public void PopulateWaterMeters()
    {
        foreach (var item in energyManager.EnergyMeters)
        {
            if(item.day_average.Count > 0)
            {
                Debug.Log($"{item.meterid} has day average of {item.day_average[0].ptot_kw} on {item.day_average[0].timestamp}");
            } 
        }

        foreach (var item in energyManager.EnergyMeters)
        {
            if (item.month_average.Count > 0)
            {
                Debug.Log($"{item.meterid} has month average of {item.month_average[0].ptot_kw} on {item.month_average[0].timestamp}");
            }
        }
    }

}
