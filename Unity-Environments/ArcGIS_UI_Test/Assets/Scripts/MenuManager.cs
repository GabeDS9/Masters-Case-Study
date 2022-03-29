using System;
using System.Collections;
using System.Collections.Generic;
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
    public TMP_Text temp;

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

    public void SpawnEnergyObjects()
    {
        int meterid = Int32.Parse(EnergyMeterListDropDown.options[EnergyMeterListDropDown.value].text);
        mapSpawnner.PopulateEnergyObject(meterid, appManager.energyManager, "2022-03-25");
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
}