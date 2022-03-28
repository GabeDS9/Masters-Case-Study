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
