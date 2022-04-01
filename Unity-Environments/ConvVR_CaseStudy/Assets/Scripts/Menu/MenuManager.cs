using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour

{
    public GameObject MainMenu;
    public GameObject MeterMenu;
    public GameObject VisualisationMenu;
    public GameObject VisualisationUI;
    public GameObject StartCalendar;
    public GameObject EndCalendar;
    public GameObject EnergyMetersButton;
    public GameObject OccupancyMetersButton;
    public GameObject SolarMetersButton;
    public TMP_Dropdown MeterListDropDown;
    public TMP_InputField startDate;
    public TMP_InputField endDate;
    public Slider visualisationSlider;
    public TMP_Text readyIndicator;
    public Button dayVisualisationButton;
    public Button monthVisualisationButton;

    public ApplicationManager appManager;
    public Mapbox.Examples.SpawnOnMap mapSpawnner;
    public LegendManager legendManager;

    private GameObject currentMenu;
    private EnergyMeterData tempEnergyMeter;
    private int meterid;
    private List<EnergyAverage> dayAverage;
    private List<EnergyAverage> monthAverage;
    private bool isDayButtonPressed = false;
    private bool isMonthButtonPressed = false;
    private bool visIsReady = false;
    private List<Toggle> legendList;

    // Start is called before the first frame update
    void Start()
    {
        MeterMenu.SetActive(false);
        StartCalendar.SetActive(false);
        EndCalendar.SetActive(false);
        EnergyMetersButton.SetActive(false);
        OccupancyMetersButton.SetActive(false);
        SolarMetersButton.SetActive(false);
        VisualisationMenu.SetActive(false);
        VisualisationUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplayMainMenu()
    {
        MeterMenu.SetActive(true);
        currentMenu.SetActive(false);
        currentMenu = MeterMenu;
        isDayButtonPressed = false;
        isMonthButtonPressed = false;
        visIsReady = false;
    }

    #region EnergyFields
    public void EnergyMeters()
    {
        MeterMenu.SetActive(true);
        currentMenu = MeterMenu;
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

        MeterListDropDown.AddOptions(options);
    }

    public void SpawnEnergyObjects(string date, List<EnergyAverage> averageList, string visualisation_type)
    {
        mapSpawnner.PopulateEnergyObject(tempEnergyMeter, averageList, date, visualisation_type);
    }

    #endregion

    #region OccupancyFields
    public void OccupancyMeters()
    {
        MeterMenu.SetActive(true);
        currentMenu = MeterMenu;
        MainMenu.SetActive(false);
        PopulateOccupancyDropdown(appManager.OccupancyMeterList);
    }
    public void PopulateOccupancyDropdown(List<OccupancyMeterData> OccupancyMeterList)
    {
        List<String> options = new List<String>();

        foreach (var item in OccupancyMeterList)
        {
            options.Add(item.meterid.ToString());
        }

        MeterListDropDown.AddOptions(options);
    }

    public void SpawnOccupancyObjects(string date, List<EnergyAverage> averageList, string visualisation_type)
    {
        mapSpawnner.PopulateEnergyObject(tempEnergyMeter, averageList, date, visualisation_type);
    }
    #endregion

    #region SolarFields
    public void SolarMeters()
    {
        MeterMenu.SetActive(true);
        currentMenu = MeterMenu;
        MainMenu.SetActive(false);
        PopulateSolarDropdown(appManager.SolarMeterList);
    }
    public void PopulateSolarDropdown(List<SolarMeterData> SolarMeterList)
    {
        List<String> options = new List<String>();

        foreach (var item in SolarMeterList)
        {
            options.Add(item.meterid.ToString());
        }

        MeterListDropDown.AddOptions(options);
    }

    public void SpawnSolarObjects(string date, List<EnergyAverage> averageList, string visualisation_type)
    {
        mapSpawnner.PopulateEnergyObject(tempEnergyMeter, averageList, date, visualisation_type);
    }
    #endregion

    #region Visualisations

    public void VisualisationsMenu()
    {
        currentMenu.SetActive(false);
        VisualisationMenu.SetActive(true);
        currentMenu = VisualisationMenu;
        VisualisationDataCalculations();
        dayVisualisationButton.enabled = visIsReady;
        monthVisualisationButton.enabled = visIsReady;
        readyIndicator.text = "retrieving information....";
    }

    private async void VisualisationDataCalculations()
    {
        String startdate = startDate.text;
        String enddate = endDate.text;
        meterid = Int32.Parse(MeterListDropDown.options[MeterListDropDown.value].text);

        Debug.Log("Obtaining data from " + startdate + " to " + enddate + " for " + meterid);

        await Task.Run(() => appManager.energyManager.CalculateDayAverage(startdate, enddate, meterid));

        await Task.Run(() => appManager.energyManager.CalculateMonthAverage(startdate, enddate, meterid));

        foreach (var item in appManager.energyManager.EnergyMeters)
        {
            if (item.meterid == meterid)
            {

                tempEnergyMeter = item;

                /*foreach (var res in item.day_average)
                {
                    Debug.Log($"Day average for {meterid} on {res.timestamp} is {res.ptot_kw}");
                }

                foreach (var res in item.month_average)
                {
                    Debug.Log($"Month average for {meterid} on {res.timestamp} is {res.ptot_kw}");
                }*/

                break;
            }
        }

        readyIndicator.text = "Visualisation ready";
        visIsReady = true;
        dayVisualisationButton.enabled = visIsReady;
        monthVisualisationButton.enabled = visIsReady;
        Debug.Log("Information is obtained");
    }

    public void DayVisualisation()
    {
        currentMenu.SetActive(false);
        VisualisationUI.SetActive(true);
        currentMenu = VisualisationUI;
        isDayButtonPressed = true;

        foreach (var item in appManager.energyManager.EnergyMeters)
        {
            if (item.meterid == meterid)
            {
                visualisationSlider.maxValue = item.day_average.Count - 1;
                Debug.Log("Slider should have " + item.day_average.Count);
                dayAverage = item.day_average;
                break;
            }
        }

        SpawnEnergyObjects(dayAverage[0].timestamp, dayAverage, "day_visualisation");
                 
    }

    public void MonthVisualisation()
    {
        currentMenu.SetActive(false);
        VisualisationUI.SetActive(true);
        currentMenu = VisualisationUI;
        isMonthButtonPressed = true;

        foreach (var item in appManager.energyManager.EnergyMeters)
        {
            if (item.meterid == meterid)
            {
                visualisationSlider.maxValue = item.month_average.Count - 1;
                Debug.Log("Slider should have " + item.month_average.Count);
                monthAverage = item.month_average;
                break;
            }
        }

        SpawnEnergyObjects(monthAverage[0].timestamp, monthAverage, "month_visualisation");

    }

    public void ChangeSlider()
    {
        if (isDayButtonPressed)
        {
            SpawnEnergyObjects(dayAverage[(int)visualisationSlider.value].timestamp, dayAverage, "day_visualisation");
        }
        else if (isMonthButtonPressed)
        {
            SpawnEnergyObjects(monthAverage[(int)visualisationSlider.value].timestamp, monthAverage, "month_visualisation");
        }
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
