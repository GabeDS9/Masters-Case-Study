using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour

{
    // Main Menu UI
    public GameObject MainMenu;
    public Dropdown CampusDropdown;
    public Dropdown PrecinctDropdown;
    public Dropdown BuildingDropdown;
    public Button MMNextButton;

    // Display Level UI
    public GameObject DisplayLevel;
    public Button HighestLevel;
    public Button MiddleLevel;
    public Button LowestLevel;
    public Button DisplayLevelMMButton;

    // Data Type UI
    public GameObject DataType;
    public Button EnergyButton;
    public Button AllButton;
    public Button DataTypeMMButton;

    // Date Select UI
    public GameObject DateSelectMenu;
    public TMP_InputField startDate;
    public TMP_InputField endDate;
    public GameObject StartCalendar;
    public GameObject EndCalendar;
    public Button DateSelectNextButton;
    public Dropdown TimePeriod;
    public Button DateSelectMMButton;

    // Visualisation UI
    public GameObject VisualisationUI;
    public Slider visualisationSlider;
    public Text Information;

    public Mapbox.Examples.SpawnOnMap mapSpawnner;

    private GameObject currentMenu;
    private ClientSocket myClient = new ClientSocket();
    private MessageHandler messHandler = new MessageHandler();

    // Flags
    private bool HighestLevelSelected = false;
    private bool MiddleLevelSelected = false;
    private bool LowestLevelSelected = false;
    private bool AllLevelSelected = false;
    private bool EnergyDataFlag = false;
    private bool AllDataFlag = false;

    // Message parameters
    private string SelectedDT = "";
    private string DTLevelSelected = "";
    private string DataTypeSelected = "";
    private string InformationTypeSelected = "";
    private string DisplayTypeSelected = "";
    private string StartDateSelected = "";
    private string EndDateSelected = "";
    private string TimePeriodSelected = "";

    private int ServerPort = 9000;

    // Start is called before the first frame update
    void Start()
    {
        MainMenu.SetActive(true);
        DisplayLevel.SetActive(false);
        DataType.SetActive(false);
        DateSelectMenu.SetActive(false);
        VisualisationUI.SetActive(false);
        PopulateDropDown();
    }

    public async void PopulateDropDown()
    {
        if (CampusDropdown.value == 0)
        {
            var dtList = await messHandler.GetDTListAsync("");
            CampusDropdown.AddOptions(dtList);
            foreach (var item in dtList)
            {
                Debug.Log(item);
            }
        }
        else if ((CampusDropdown.value != 0) && (PrecinctDropdown.value == 0))
        {
            //Debug.Log(CampusDropdown.options[CampusDropdown.value].text);
            var dtList = await messHandler.GetDTListAsync(CampusDropdown.options[CampusDropdown.value].text);
            PrecinctDropdown.AddOptions(dtList);
            foreach (var item in dtList)
            {
                Debug.Log(item);
            }
        }
        else if ((CampusDropdown.value != 0) && (PrecinctDropdown.value != 0) && (BuildingDropdown.value == 0))
        {
            //Debug.Log(PrecinctDropdown.options[PrecinctDropdown.value].text);
            var dtList = await messHandler.GetDTListAsync(PrecinctDropdown.options[PrecinctDropdown.value].text);
            BuildingDropdown.AddOptions(dtList);
            foreach (var item in dtList)
            {
                Debug.Log(item);
            }
        }
    }

    public void DisplayMainMenu()
    {
        CampusDropdown.value = 0;
        PrecinctDropdown.value = 0;
        BuildingDropdown.value = 0;
        CampusDropdown.ClearOptions();
        PrecinctDropdown.ClearOptions();
        BuildingDropdown.ClearOptions();
        List<string> tempCampOptions = new List<string>();
        tempCampOptions.Add("Select Campus");
        CampusDropdown.AddOptions(tempCampOptions);
        List<string> tempPrecOptions = new List<string>();
        tempPrecOptions.Add("Select Precinct");
        PrecinctDropdown.AddOptions(tempPrecOptions);
        List<string> tempBuildOptions = new List<string>();
        tempBuildOptions.Add("Select Building");
        BuildingDropdown.AddOptions(tempBuildOptions);
        PopulateDropDown();
        MainMenu.SetActive(true);
        currentMenu.SetActive(false);
        currentMenu = MainMenu;
    }
    public void DisplayDTLevelMenu()
    {
        HighestLevelSelected = false;
        MiddleLevelSelected = false;
        LowestLevelSelected = false;
        AllLevelSelected = false;
        HighestLevel.gameObject.SetActive(false);
        MiddleLevel.gameObject.SetActive(false);
        LowestLevel.gameObject.SetActive(false);

        if (CampusDropdown.value != 0)
        {
            if (PrecinctDropdown.value != 0)
            {
                if (BuildingDropdown.value != 0)
                {
                    SelectedDT = BuildingDropdown.options[BuildingDropdown.value].text;
                    HighestLevel.GetComponentInChildren<Text>().text = "Building";
                    HighestLevel.gameObject.SetActive(true);
                    MiddleLevel.gameObject.SetActive(false);
                    LowestLevel.gameObject.SetActive(false);
                }
                else
                {
                    SelectedDT = PrecinctDropdown.options[PrecinctDropdown.value].text;
                    HighestLevel.GetComponentInChildren<Text>().text = "Precinct";
                    MiddleLevel.GetComponentInChildren<Text>().text = "Building";
                    HighestLevel.gameObject.SetActive(true);
                    MiddleLevel.gameObject.SetActive(true);
                    LowestLevel.gameObject.SetActive(false);
                }
            }
            else
            {
                SelectedDT = CampusDropdown.options[CampusDropdown.value].text;
                HighestLevel.GetComponentInChildren<Text>().text = "Campus";
                MiddleLevel.GetComponentInChildren<Text>().text = "Precinct";
                LowestLevel.GetComponentInChildren<Text>().text = "Building";
                HighestLevel.gameObject.SetActive(true);
                MiddleLevel.gameObject.SetActive(true);
                LowestLevel.gameObject.SetActive(true);
            }

            MainMenu.SetActive(false);
            DisplayLevel.SetActive(true);
            currentMenu = DisplayLevel;
        }
        else
        {
            DisplayMainMenu();
        }
    }
    public void DisplayDataTypeMenu()
    {
        Debug.Log(DTLevelSelected);
        EnergyDataFlag = false;
        AllDataFlag = false;
        DataTypeSelected = "";
        if (HighestLevelSelected)
        {
            DTLevelSelected = HighestLevel.GetComponentInChildren<Text>().text;
            DisplayTypeSelected = "Individual";
        }
        else if (MiddleLevelSelected)
        {
            DTLevelSelected = MiddleLevel.GetComponentInChildren<Text>().text;
            DisplayTypeSelected = "Collective";
        }
        else if (LowestLevelSelected)
        {
            DTLevelSelected = LowestLevel.GetComponentInChildren<Text>().text;
            DisplayTypeSelected = "Collective";
        }
        else if (AllLevelSelected)
        {
            DTLevelSelected = "All";
            DisplayTypeSelected = "Collective";
        }

        currentMenu.SetActive(false);
        DataType.SetActive(true);
        currentMenu = DataType;        
    }
    public void DisplayTimeSelectMenu()
    {
        Debug.Log(DataTypeSelected);
        DisableStartCalendar();
        DisableEndCalendar();
        currentMenu.SetActive(false);
        DateSelectMenu.SetActive(true);
        currentMenu = DateSelectMenu;
    }
    public void DisplayVisualisationUI()
    {
        currentMenu.SetActive(false);
        VisualisationUI.SetActive(true);
        currentMenu = VisualisationUI;
    }

    // Setting DT Level Flags
    public void SetHighestLevelFlag()
    {
        HighestLevelSelected = true;
    }
    public void SetMiddleLevelFlag()
    {
        MiddleLevelSelected = true;
    }
    public void SetLowestLevelFlag()
    {
        LowestLevelSelected = true;
    }
    public void SetAllLevelFlag()
    {
        AllLevelSelected = true;
    }

    // Setting Data Type Flag
    public void SetEnergyDataFlag()
    {
        EnergyDataFlag = true;
        DataTypeSelected = "Energy";
    }
    public void SetAllDataFlag()
    {
        AllDataFlag = true;
        DataTypeSelected = "All";
    }
    public async void GetCurrentInformation()
    {
        TimePeriodSelected = "";
        StartDateSelected = "";
        EndDateSelected = "";
        InformationTypeSelected = "CurrentData";
        var message = CreateMessage();
        DisplayVisualisationUI();
        var response = await myClient.sendMessageAsync(message, ServerPort);
        Information.text = response;
    }
    public async void GetInformation()
    {
        TimePeriodSelected = TimePeriod.options[TimePeriod.value].text;
        StartDateSelected = startDate.text;
        EndDateSelected = endDate.text;
        InformationTypeSelected = "Averages";
        var message = CreateMessage();
        DisplayVisualisationUI();
        var response = await myClient.sendMessageAsync(message, ServerPort);
        Information.text = response;
    }
    private string CreateMessage()
    {
        var message = new MessageModel
        {
            DataType = DataTypeSelected,
            InformationType = InformationTypeSelected,
            DisplayType = DisplayTypeSelected,
            DigitalTwin = SelectedDT,
            LowestDTLevel = DTLevelSelected,
            startDate = StartDateSelected,
            endDate = EndDateSelected,
            timePeriod = TimePeriodSelected
        };
        var mes = JsonConvert.SerializeObject(message);
        Debug.Log(mes);
        return mes;        
    }

    public void DisplayStartCalendar()
    {
        StartCalendar.SetActive(true);
        startDate.text = "";
    }

    public void DisableStartCalendar()
    {
        StartCalendar.SetActive(false);
    }

    public void DisplayEndCalendar()
    {
        EndCalendar.SetActive(true);
        endDate.text = "";
    }

    public void DisableEndCalendar()
    {
        EndCalendar.SetActive(false);
    }

    /*#region EnergyFields
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
                }

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
    }*/


}
