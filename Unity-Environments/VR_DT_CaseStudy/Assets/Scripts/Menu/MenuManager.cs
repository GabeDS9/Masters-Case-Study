using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

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
    public Toggle HighestLevelToggle;
    public Toggle MiddleLevelToggle;
    public Toggle LowestLevelToggle;
    public Button DisplayLevelMMButton;
    public Button DisplayLevelNextButton;

    // Data Type UI
    public GameObject DataType;
    public Button EnergyButton;
    public Button AllButton;
    public Button DataTypeMMButton;

    // Services Select UI
    public GameObject ServicesSelectMenu;

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
    public Text visualisationStatus;
    public Slider visualisationScale;
    public Button PlayUIButton;

    public Mapbox.Examples.SpawnOnMap mapSpawnner;

    private GameObject currentMenu;
    private ClientSocket myClient = new ClientSocket();
    private MessageHandler messHandler = new MessageHandler();
    private Utilities utils = new Utilities();

    // Flags
    private bool AllLevelSelected = false;
    private bool EnergyDataFlag = false;
    private bool AllDataFlag = false;

    // Message parameters
    private string SelectedDT = "";
    private List<string> DTLevelSelected = new List<string>();
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
        ServicesSelectMenu.SetActive(false);
        DateSelectMenu.SetActive(false);
        VisualisationUI.SetActive(false);
        PrecinctDropdown.interactable = false;
        BuildingDropdown.interactable = false;
        MMNextButton.interactable = false;
        currentMenu = MainMenu;
        DisplayMainMenu();        
    }

    public async void PopulateDropDown()
    {
        if (CampusDropdown.value == 0)
        {
            var dtList = await messHandler.GetDTListAsync("");
            CampusDropdown.AddOptions(dtList);
        }
        else if ((CampusDropdown.value != 0) && (PrecinctDropdown.value == 0))
        {
            //Debug.Log(CampusDropdown.options[CampusDropdown.value].text);
            PrecinctDropdown.interactable = true;
            MMNextButton.interactable = true;
            var dtList = await messHandler.GetDTListAsync(CampusDropdown.options[CampusDropdown.value].text);
            PrecinctDropdown.AddOptions(dtList);
        }
        else if ((CampusDropdown.value != 0) && (PrecinctDropdown.value != 0) && (BuildingDropdown.value == 0))
        {
            //Debug.Log(PrecinctDropdown.options[PrecinctDropdown.value].text);
            BuildingDropdown.interactable = true;
            var dtList = await messHandler.GetDTListAsync(PrecinctDropdown.options[PrecinctDropdown.value].text);
            BuildingDropdown.AddOptions(dtList);
        }
    }
    public void ResetDropDown()
    {
        CampusDropdown.ClearOptions();
        PrecinctDropdown.ClearOptions();
        BuildingDropdown.ClearOptions();
        PrecinctDropdown.interactable = false;
        BuildingDropdown.interactable = false;
        MMNextButton.interactable = false;
        List<string> tempCampOptions = new List<string>();
        tempCampOptions.Add("Select Campus");
        CampusDropdown.AddOptions(tempCampOptions);
        List<string> tempPrecOptions = new List<string>();
        tempPrecOptions.Add("Select Precinct");
        PrecinctDropdown.AddOptions(tempPrecOptions);
        List<string> tempBuildOptions = new List<string>();
        tempBuildOptions.Add("Select Building");
        BuildingDropdown.AddOptions(tempBuildOptions);
        CampusDropdown.value = 0;
        PrecinctDropdown.value = 0;
        BuildingDropdown.value = 0;
    }
    public void DisplayMainMenu()
    {
        CancelInvoke();
        ResetDropDown();
        PopulateDropDown();
        currentMenu.SetActive(false);
        MainMenu.SetActive(true);        
        currentMenu = MainMenu;
        TimePeriod.value = 0;
    }
    public void DisplayDTLevelMenu()
    {
        AllLevelSelected = false;
        HighestLevelToggle.gameObject.SetActive(false);
        MiddleLevelToggle.gameObject.SetActive(false);
        LowestLevelToggle.gameObject.SetActive(false);
        HighestLevelToggle.isOn = false;
        MiddleLevelToggle.isOn = false;
        LowestLevelToggle.isOn = false;
        DTLevelSelected.Clear();
        DisplayLevelNextButton.interactable = false;

        if (CampusDropdown.value != 0)
        {
            if (PrecinctDropdown.value != 0)
            {
                if (BuildingDropdown.value != 0)
                {
                    SelectedDT = BuildingDropdown.options[BuildingDropdown.value].text;
                    HighestLevelToggle.GetComponentInChildren<Text>().text = "Building";
                    HighestLevelToggle.gameObject.SetActive(true);
                    MiddleLevelToggle.gameObject.SetActive(false);
                    LowestLevelToggle.gameObject.SetActive(false);
                }
                else
                {
                    SelectedDT = PrecinctDropdown.options[PrecinctDropdown.value].text;
                    HighestLevelToggle.GetComponentInChildren<Text>().text = "Precinct";
                    MiddleLevelToggle.GetComponentInChildren<Text>().text = "Building";
                    HighestLevelToggle.gameObject.SetActive(true);
                    MiddleLevelToggle.gameObject.SetActive(true);
                    LowestLevelToggle.gameObject.SetActive(false);
                }
            }
            else
            {
                SelectedDT = CampusDropdown.options[CampusDropdown.value].text;
                HighestLevelToggle.GetComponentInChildren<Text>().text = "Campus";
                MiddleLevelToggle.GetComponentInChildren<Text>().text = "Precinct";
                LowestLevelToggle.GetComponentInChildren<Text>().text = "Building";
                HighestLevelToggle.gameObject.SetActive(true);
                MiddleLevelToggle.gameObject.SetActive(true);
                LowestLevelToggle.gameObject.SetActive(true);
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
        EnergyDataFlag = false;
        AllDataFlag = false;
        DataTypeSelected = "";
        AllButton.interactable = false;
        if (HighestLevelToggle.isOn && !MiddleLevelToggle.isOn && !LowestLevelToggle.isOn)
        {
            DTLevelSelected.Add(HighestLevelToggle.GetComponentInChildren<Text>().text);
            DisplayTypeSelected = "Individual";
        }
        else if (HighestLevelToggle.isOn)
        {
            DTLevelSelected.Add(HighestLevelToggle.GetComponentInChildren<Text>().text);
            DisplayTypeSelected = "Collective";
        }
        if (MiddleLevelToggle.isOn)
        {
            DTLevelSelected.Add(MiddleLevelToggle.GetComponentInChildren<Text>().text);
            DisplayTypeSelected = "Collective";
        }
        if (LowestLevelToggle.isOn)
        {
            DTLevelSelected.Add(LowestLevelToggle.GetComponentInChildren<Text>().text);
            DisplayTypeSelected = "Collective";
        }
        if (AllLevelSelected)
        {
            DTLevelSelected.Clear();
            DTLevelSelected.Add("All");
            DisplayTypeSelected = "Collective";
        }

        currentMenu.SetActive(false);
        DataType.SetActive(true);
        currentMenu = DataType;        
    }
    public void DisplayServicesSelectMenu()
    {
        currentMenu.SetActive(false);
        ServicesSelectMenu.SetActive(true);
        currentMenu = ServicesSelectMenu;
    }
    public void DisplayTimeSelectMenu()
    {
        TimePeriod.value = 0;
        startDate.text = "";
        endDate.text = "";
        DateSelectNextButton.interactable = false;
        DisableStartCalendar();
        DisableEndCalendar();
        currentMenu.SetActive(false);
        DateSelectMenu.SetActive(true);
        currentMenu = DateSelectMenu;
    }
    public void DisplayVisualisationUI()
    {
        PlayUIButton.interactable = false;
        PlayUIButton.GetComponentInChildren<Text>().text = "Play";
        visualisationSlider.value = 0;
        visualisationSlider.interactable = false;
        visualisationScale.interactable = true;
        visualisationScale.maxValue = 2;
        visualisationScale.minValue = 0.1f;
        visualisationScale.value = 1;
        currentMenu.SetActive(false);
        VisualisationUI.SetActive(true);
        currentMenu = VisualisationUI;
        if(TimePeriod.value != 0)
        {
            PlayUIButton.interactable = true;
            ConfigureVisualisationSliders();
        }        
    }
    public void ChangeVisualisationDate()
    {
        if (visualisationSlider.value != 0)
        {
            var date = utils.GenerateDateList(StartDateSelected, EndDateSelected, TimePeriodSelected)[((int)visualisationSlider.value)];
            mapSpawnner.ChangeVisualisationDate(date);
        }
    }
    public void ChangeVisualisationScale()
    {
        var scale = visualisationScale.value;
        mapSpawnner.ChangeVisualisationScale(scale);
    }
    public void PlayVisualisation()
    {
        if(PlayUIButton.GetComponentInChildren<Text>().text == "Play")
        {
            visualisationSlider.interactable = false;
            PlayUIButton.GetComponentInChildren<Text>().text = "Stop";
            InvokeRepeating("PlayFunction", 0.2f, 0.2f);
        }
        else if (PlayUIButton.GetComponentInChildren<Text>().text == "Stop")
        {
            visualisationSlider.interactable = true;
            PlayUIButton.GetComponentInChildren<Text>().text = "Play";
            CancelInvoke();
        }
    }
    private void PlayFunction()
    {
        var val = visualisationSlider.value + 1;
        if(val > visualisationSlider.maxValue)
        {
            visualisationSlider.value = visualisationSlider.minValue;
        }
        else
        {
            visualisationSlider.value = val;
        }
        ChangeVisualisationDate();
    }
    private void ConfigureVisualisationSliders()
    {
        visualisationSlider.interactable = true;
        visualisationSlider.wholeNumbers = true;        
        var numDates = utils.GenerateDateList(StartDateSelected, EndDateSelected, TimePeriodSelected).Count - 1;
        visualisationSlider.maxValue = numDates;
    }
    // Setting DT Level Flags
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
        visualisationStatus.text = "Getting visualisation ready...";
        TimePeriodSelected = "";
        StartDateSelected = "";
        EndDateSelected = "";
        InformationTypeSelected = "CurrentData";
        var message = CreateMessage();
        DisplayVisualisationUI();
        var response = await myClient.sendMessageAsync(message, ServerPort);
        Debug.Log("Length of response was " + response.Length);
        mapSpawnner.PopulateData(response, null);
        visualisationStatus.text = "Visualisation ready";
    }
    public async void GetInformation()
    {
        visualisationStatus.text = "Getting visualisation ready...";
        TimePeriodSelected = TimePeriod.options[TimePeriod.value].text;
        StartDateSelected = startDate.text;
        EndDateSelected = endDate.text;
        InformationTypeSelected = "Averages";
        var message = CreateMessage();
        DisplayVisualisationUI();
        var response = await myClient.sendMessageAsync(message, ServerPort);
        var DateList = utils.GenerateDateList(StartDateSelected, EndDateSelected, TimePeriodSelected);
        mapSpawnner.PopulateData(response, DateList);
        visualisationStatus.text = "Visualisation ready";
    }
    private string CreateMessage()
    {
        var message = new MessageModel
        {
            ServiceTag = "Exploratory",
            DataType = DataTypeSelected,
            InformationType = InformationTypeSelected,
            DisplayType = DisplayTypeSelected,
            DigitalTwin = SelectedDT,
            DTDetailLevel = DTLevelSelected,
            startDate = StartDateSelected,
            endDate = EndDateSelected,
            timePeriod = TimePeriodSelected
        };
        var mes = JsonConvert.SerializeObject(message);
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
    public void ChangeDisplayLevelNext()
    {
        DisplayLevelNextButton.interactable = true;
    }
    public void ChangeTimePeriodNext()
    {
        if((startDate.text != "") && (endDate.text != "") && (TimePeriod.value != 0))
        {
            DateSelectNextButton.interactable = true;
        }        
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
