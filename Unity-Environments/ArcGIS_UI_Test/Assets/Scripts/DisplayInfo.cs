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
        foreach(var record in energyManager.EnergyMeters)
        {
            spawnOnMap.PopulateEnergyObject(record.meterid, energyManager);
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
