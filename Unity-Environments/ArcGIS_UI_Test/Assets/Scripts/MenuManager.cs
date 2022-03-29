using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour

{
    public GameObject MainMenu;
    public GameObject MeterMenu;
    public GameObject StartCalendar;
    public GameObject EndCalendar;
    public GameObject EnergyMetersButton;
    public TMP_Dropdown EnergyMeterListDropDown;
    public TMP_InputField startDate;
    public TMP_InputField endDate;

    public ApplicationManager appManager;
    public Mapbox.Examples.SpawnOnMap mapSpawnner;

    // Start is called before the first frame update
    void Start()
    {
        MeterMenu.SetActive(false);
        StartCalendar.SetActive(false);
        EndCalendar.SetActive(false);
        EnergyMetersButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region EnergyFields
    public void EnergyMeters()
    {
        MeterMenu.SetActive(true);
        MainMenu.SetActive(false);
        PopulateEnergyDropdown(appManager.EnergyMeterList);
    }
    public void PopulateEnergyDropdown(List<EnergyMeterData> EnergyMeterList)
    {
        List<String> options = new List<String>();

        foreach(var item in EnergyMeterList)
        {
            options.Add(item.meterid.ToString());
        }

        EnergyMeterListDropDown.AddOptions(options);
    }

    public async void SpawnEnergyObjectsAsync()
    {
        Debug.Log("Spawning object");
        int meterid = Int32.Parse(EnergyMeterListDropDown.options[EnergyMeterListDropDown.value].text);
        await mapSpawnner.PopulateEnergyObjectAsync(meterid, appManager.energyManager, "2022-03-27");
    }

    #endregion

    public void DisplayStartCalendar()
    {
        StartCalendar.SetActive(true);
    }

    public void DisableStartCalendar()
    {
        StartCalendar.SetActive(false);
    }

    public void DisplayEndCalendar()
    {
        EndCalendar.SetActive(true);
    }

    public void DisableEndCalendar()
    {
        EndCalendar.SetActive(false);
    }

    public async void VisualiseDataAsync()
    {
        String startdate = startDate.text;
        String enddate = endDate.text;
        int meterID = Int32.Parse(EnergyMeterListDropDown.options[EnergyMeterListDropDown.value].text);

        Debug.Log("Obtaining data from " + startdate + " to " + enddate + " for " + meterID);

        await Task.Run(() => appManager.energyManager.CalculateDayAverage(startdate, enddate, meterID));

        await Task.Run(() => appManager.energyManager.CalculateMonthAverage(startdate, enddate, meterID));

        foreach (var item in appManager.energyManager.EnergyMeters)
        {
            if (item.meterid == meterID)
            {
                foreach (var res in item.day_average)
                {
                    Debug.Log($"Day average for {meterID} on {res.timestamp} is {res.ptot_kw}");
                }

                foreach (var res in item.month_average)
                {
                    Debug.Log($"Month average for {meterID} on {res.timestamp} is {res.ptot_kw}");
                }

                break;
            }
        }
    }
}
