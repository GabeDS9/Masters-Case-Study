using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Models;
using System.Diagnostics;

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
    private Utilities utils = new Utilities();

    // Flags
    private bool AllLevelSelected = false;
    private bool EnergyDataFlag = false;
    private bool AllDataFlag = false;

    private string SelectedElement;
    private List<string> DTLevelSelected = new List<string>();
    private string DataTypeSelected = "";
    private string InformationTypeSelected = "";
    private string DisplayTypeSelected = "";
    private string StartDateSelected = "";
    private string EndDateSelected = "";
    private string TimePeriodSelected = "";

    private List<ElementModel> CampusList = new List<ElementModel>();
    private List<ElementModel> PrecinctList = new List<ElementModel>();
    private List<ElementModel> BuildingList = new List<ElementModel>();

    public LoadExcel excel;
    private InformationHandler infoHandler = new InformationHandler();

    // Evaluation parameters
    Stopwatch stopwatch = new Stopwatch();
    Stopwatch dataStopwatch = new Stopwatch();
    Stopwatch visStopwatch = new Stopwatch();
    public CSVWriter csvWriter;
    //public Test tester = new Test();
    public MemoryProfiler memoryProfiler;

    // Start is called before the first frame update
    void Start()
    {
        LoadElements();
        MainMenu.SetActive(false);
        DisplayLevel.SetActive(false);
        DataType.SetActive(false);
        ServicesSelectMenu.SetActive(false);
        DateSelectMenu.SetActive(false);
        VisualisationUI.SetActive(true);
        PrecinctDropdown.interactable = false;
        BuildingDropdown.interactable = false;
        MMNextButton.interactable = false;
        currentMenu = MainMenu;
        _ = RunEvaluationTest();
    }
    private void LoadElements()
    {
        CampusList = excel.LoadCampus();
        PrecinctList = excel.LoadPrecincts();
        BuildingList = excel.LoadBuildings();
    }
    public void DisplayMainMenu()
    {
        CancelInvoke();
        ResetDropDown();
        PopulateDropDown();
        currentMenu.SetActive(false);
        MainMenu.SetActive(true);
        currentMenu = MainMenu;
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
    public void PopulateDropDown()
    {
        if (CampusDropdown.value == 0)
        {
            List<string> dtList = new List<string>();
            foreach (var camp in CampusList)
            {
                dtList.Add(camp.ElementName);
            }
            CampusDropdown.AddOptions(dtList);
        }
        else if ((CampusDropdown.value != 0) && (PrecinctDropdown.value == 0))
        {
            PrecinctDropdown.interactable = true;
            MMNextButton.interactable = true;
            List<string> dtList = new List<string>();
            foreach (var prec in PrecinctList)
            {
                dtList.Add(prec.ElementName);
            }
            PrecinctDropdown.AddOptions(dtList);
        }
        else if ((CampusDropdown.value != 0) && (PrecinctDropdown.value != 0) && (BuildingDropdown.value == 0))
        {
            BuildingDropdown.interactable = true;
            List<string> dtList = new List<string>();
            foreach (var prec in PrecinctList)
            {
                if (prec.ElementName == PrecinctDropdown.options[PrecinctDropdown.value].text)
                {
                    foreach (var child in prec.ChildElements)
                    {
                        foreach (var build in BuildingList)
                        {
                            if (child == build.ElementName)
                            {
                                dtList.Add(build.ElementName);
                            }
                        }
                    }
                }
            }
            BuildingDropdown.AddOptions(dtList);
        }
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
        DisplayLevelNextButton.interactable = false;

        if (CampusDropdown.value != 0)
        {
            if (PrecinctDropdown.value != 0)
            {
                if (BuildingDropdown.value != 0)
                {
                    SelectedElement = BuildingDropdown.options[BuildingDropdown.value].text;
                    HighestLevelToggle.GetComponentInChildren<Text>().text = "Building";
                    HighestLevelToggle.gameObject.SetActive(true);
                    MiddleLevelToggle.gameObject.SetActive(false);
                    LowestLevelToggle.gameObject.SetActive(false);
                }
                else
                {
                    SelectedElement = PrecinctDropdown.options[PrecinctDropdown.value].text;
                    HighestLevelToggle.GetComponentInChildren<Text>().text = "Precinct";
                    MiddleLevelToggle.GetComponentInChildren<Text>().text = "Building";
                    HighestLevelToggle.gameObject.SetActive(true);
                    MiddleLevelToggle.gameObject.SetActive(true);
                    LowestLevelToggle.gameObject.SetActive(false);
                }
            }
            else
            {
                SelectedElement = CampusDropdown.options[CampusDropdown.value].text;
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
        DTLevelSelected.Clear();
        if (LowestLevelToggle.isOn)
        {
            DTLevelSelected.Add(LowestLevelToggle.GetComponentInChildren<Text>().text);
            DisplayTypeSelected = "Collective";
        }
        if (MiddleLevelToggle.isOn)
        {
            DTLevelSelected.Add(MiddleLevelToggle.GetComponentInChildren<Text>().text);
            DisplayTypeSelected = "Collective";
        }
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

        if (AllLevelSelected)
        {
            DTLevelSelected.Clear();
            if (LowestLevelToggle.IsActive())
            {
                DTLevelSelected.Add("Building");
                DTLevelSelected.Add("Precinct");
                DTLevelSelected.Add("Campus");
            }
            else if (!LowestLevelToggle.IsActive() && MiddleLevelToggle.IsActive())
            {
                DTLevelSelected.Add("Building");
                DTLevelSelected.Add("Precinct");
            }
            else if (!LowestLevelToggle.IsActive() && !MiddleLevelToggle.IsActive() && HighestLevelToggle.IsActive())
            {
                DTLevelSelected.Add("Building");
            }
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
        if (TimePeriod.value != 0)
        {
            PlayUIButton.interactable = true;
            ConfigureVisualisationSliders();
        }
    }
    public void ChangeVisualisationDate()
    {
        //if (visualisationSlider.value != 0)
        //{
        var date = utils.GenerateDateList(StartDateSelected, EndDateSelected, TimePeriodSelected)[((int)visualisationSlider.value)];
        mapSpawnner.ChangeVisualisationDate(date);
        //}
    }
    public void ChangeVisualisationScale()
    {
        var scale = visualisationScale.value;
        mapSpawnner.ChangeVisualisationScale(scale);
    }
    public void PlayVisualisation()
    {
        if (PlayUIButton.GetComponentInChildren<Text>().text == "Play")
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
        if (val > visualisationSlider.maxValue)
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
    public async void GetCurrentInformation()
    {
        visualisationStatus.text = "Getting visualisation ready...";
        TimePeriodSelected = "";
        StartDateSelected = "";
        EndDateSelected = "";
        InformationTypeSelected = "CurrentData";
        var response = await infoHandler.GetInformationAsync(DataTypeSelected, InformationTypeSelected, DisplayTypeSelected,
            SelectedElement, DTLevelSelected, StartDateSelected, EndDateSelected, TimePeriodSelected, CampusList, PrecinctList, BuildingList);
        //Debug.Log($"{response[0].Element_Name} {response[0].Timestamp} {response[0].Value}");
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
        var response = await infoHandler.GetInformationAsync(DataTypeSelected, InformationTypeSelected, DisplayTypeSelected, SelectedElement, DTLevelSelected,
            StartDateSelected, EndDateSelected, TimePeriodSelected, CampusList, PrecinctList, BuildingList);
        var DateList = utils.GenerateDateList(StartDateSelected, EndDateSelected, TimePeriodSelected);
        mapSpawnner.PopulateData(response, DateList);
        visualisationStatus.text = "Visualisation ready";
    }
    #region EvaluationFunctions
    public async Task RunEvaluationTest()
    {
        List<EvaluationTestModel> testInformation = new List<EvaluationTestModel>();
        int numberOfCampuses = CampusList.Count;
        int numberOfPrecincts = PrecinctList.Count;
        int numberOfBuildings = BuildingList.Count;
        int numberOfDTs = numberOfCampuses + numberOfPrecincts + numberOfBuildings;

        string sDate = "2022-5-1";
        string eDate = "2022-5-5";
        var DateList = utils.GenerateDateList(sDate, eDate, "Day");
        List<string> dtLevel = new List<string>();
        #region Tests
        /* Single DTI Test with single data point (Current Data) 
        dtLevel.Add("Building");
        EvaluationTestModel temp = await GetInformation("Energy", "CurrentData", "Individual", "HIV Bestuur Gebou", dtLevel, "", "", "");
        EvaluationTestModel testInfo = new EvaluationTestModel
        {
            NumberOfDTs = 1,
            NumberOfCampuses = 0,
            NumberOfPrecincts = 0,
            NumberOfBuildings = 1,
            TotalTimeTaken = temp.TotalTimeTaken,
            DTResponseTimeTaken = temp.DTResponseTimeTaken,
            VisualTimeTaken = temp.VisualTimeTaken,
            NumberOfDataPoints = temp.NumberOfDataPoints,
            RAMusageMBTotal = temp.RAMusageMBTotal,
            RAMusagePercTotal = temp.RAMusagePercTotal,
            CPUusageTotal = temp.CPUusageTotal
        };
        testInformation.Add(testInfo);
        csvWriter.WriteCSV(testInformation, 0);
        testInformation.Clear();
        dtLevel.Clear();
        visualisationStatus.text = "Single DTI Test with single data point (Current Data) Complete";
        /* Multiple DTI Test with single data points (Current Data) */
        dtLevel.Add("Building");
        EvaluationTestModel temp = await GetInformation("Energy", "CurrentData", "Collective", "Stellenbosch University", dtLevel, "", "", "");
        EvaluationTestModel testInfo = new EvaluationTestModel
        {
            NumberOfDTs = numberOfDTs,
            NumberOfCampuses = numberOfCampuses,
            NumberOfPrecincts = numberOfPrecincts,
            NumberOfBuildings = numberOfBuildings,
            TotalTimeTaken = temp.TotalTimeTaken,
            DTResponseTimeTaken = temp.DTResponseTimeTaken,
            VisualTimeTaken = temp.VisualTimeTaken,
            NumberOfDataPoints = temp.NumberOfDataPoints,
            RAMusageMBTotal = temp.RAMusageMBTotal,
            RAMusagePercTotal = temp.RAMusagePercTotal,
            CPUusageTotal = temp.CPUusageTotal
        };        
        testInformation.Add(testInfo);
        csvWriter.WriteCSV(testInformation, 1);
        testInformation.Clear();
        dtLevel.Clear();
        visualisationStatus.text = "Multiple DTI Test with single data points (Current Data) Complete";
        /* Single DT Test with multiple data points (Averages) 
        dtLevel.Add("Building");
        foreach (var date in DateList)
        {
            temp = await GetInformation("Energy", "Averages", "Collective", "HIV Bestuur Gebou", dtLevel, sDate, date, "Day");
            testInfo = new EvaluationTestModel
            {
                NumberOfDTs = numberOfDTs,
                NumberOfCampuses = numberOfCampuses,
                NumberOfPrecincts = numberOfPrecincts,
                NumberOfBuildings = numberOfBuildings,
                TotalTimeTaken = temp.TotalTimeTaken,
                DTResponseTimeTaken = temp.DTResponseTimeTaken,
                VisualTimeTaken = temp.VisualTimeTaken,
                NumberOfDataPoints = temp.NumberOfDataPoints,
                RAMusageMBTotal = temp.RAMusageMBTotal,
                RAMusagePercTotal = temp.RAMusagePercTotal,
                CPUusageTotal = temp.CPUusageTotal
            };
            testInformation.Add(testInfo);
        }
        csvWriter.WriteCSV(testInformation, 2);
        testInformation.Clear();
        dtLevel.Clear();
        visualisationStatus.text = "Single DT Test with multiple data points (Averages) Complete";
        /* Multiple DTIs Test with multiple data points (Averages) */
        dtLevel.Add("Building");
        foreach (var date in DateList)
        {
            temp = await GetInformation("Energy", "Averages", "Collective", "Stellenbosch University", dtLevel, sDate, date, "Day");
            testInfo = new EvaluationTestModel
            {
                NumberOfDTs = numberOfDTs,
                NumberOfCampuses = numberOfCampuses,
                NumberOfPrecincts = numberOfPrecincts,
                NumberOfBuildings = numberOfBuildings,
                TotalTimeTaken = temp.TotalTimeTaken,
                DTResponseTimeTaken = temp.DTResponseTimeTaken,
                VisualTimeTaken = temp.VisualTimeTaken,
                NumberOfDataPoints = temp.NumberOfDataPoints,
                RAMusageMBTotal = temp.RAMusageMBTotal,
                RAMusagePercTotal = temp.RAMusagePercTotal,
                CPUusageTotal = temp.CPUusageTotal
            };
            testInformation.Add(testInfo);
        }
        csvWriter.WriteCSV(testInformation, 3);
        testInformation.Clear();
        dtLevel.Clear();
        visualisationStatus.text = "Multiple DTIs Test with multiple data points (Averages) Complete";
        /* Aggregate Test with single data point (Current Data) */
        dtLevel.Add("Building");
        dtLevel.Add("Precinct");
        dtLevel.Add("Campus");
        temp = await GetInformation("Energy", "CurrentData", "Collective", "Stellenbosch University", dtLevel, "", "", "");
        testInfo = new EvaluationTestModel
        {
            NumberOfDTs = numberOfDTs,
            NumberOfCampuses = numberOfCampuses,
            NumberOfPrecincts = numberOfPrecincts,
            NumberOfBuildings = numberOfBuildings,
            TotalTimeTaken = temp.TotalTimeTaken,
            DTResponseTimeTaken = temp.DTResponseTimeTaken,
            VisualTimeTaken = temp.VisualTimeTaken,
            NumberOfDataPoints = temp.NumberOfDataPoints,
            RAMusageMBTotal = temp.RAMusageMBTotal,
            RAMusagePercTotal = temp.RAMusagePercTotal,
            CPUusageTotal = temp.CPUusageTotal
        };
        testInformation.Add(testInfo);
        csvWriter.WriteCSV(testInformation, 4);
        testInformation.Clear();
        dtLevel.Clear();
        visualisationStatus.text = "Aggregate Test with single data point (Current Data) Complete";
        /* Aggregate Test with multiple data points (Averages) */
        dtLevel.Add("Building");
        dtLevel.Add("Precinct");
        dtLevel.Add("Campus");
        foreach (var date in DateList)
        {            
            temp = await GetInformation("Energy", "Averages", "Collective", "Stellenbosch University", dtLevel, sDate, date, "Day");
            testInfo = new EvaluationTestModel {
                NumberOfDTs = numberOfDTs,
                NumberOfCampuses = numberOfCampuses,
                NumberOfPrecincts = numberOfPrecincts,
                NumberOfBuildings = numberOfBuildings,
                TotalTimeTaken = temp.TotalTimeTaken,
                DTResponseTimeTaken = temp.DTResponseTimeTaken,
                VisualTimeTaken = temp.VisualTimeTaken,
                NumberOfDataPoints = temp.NumberOfDataPoints,
                RAMusageMBTotal = temp.RAMusageMBTotal,
                RAMusagePercTotal = temp.RAMusagePercTotal,
                CPUusageTotal = temp.CPUusageTotal
            };
            testInformation.Add(testInfo);
        }
        csvWriter.WriteCSV(testInformation, 5);
        testInformation.Clear();
        dtLevel.Clear();

        visualisationStatus.text = "Test Complete";
        #endregion
    }
    private async Task<EvaluationTestModel> GetInformation(string dataType, string informationType, string displayType, string digitalTwin, List<string> dtLevel,
            string sDate, string eDate, string timePer)
    {
        stopwatch.Start();
        dataStopwatch.Start();
        visualisationStatus.text = "Getting visualisation ready...";
        TimePeriodSelected = TimePeriod.options[TimePeriod.value].text;
        StartDateSelected = startDate.text;
        EndDateSelected = endDate.text;
        InformationTypeSelected = "Averages";
        var response = await infoHandler.GetInformationAsync(dataType, informationType, displayType, digitalTwin, dtLevel,
            sDate, eDate, timePer, CampusList, PrecinctList, BuildingList);
        List<string> DateList = null;
        if (sDate != "")
        {
            DateList = utils.GenerateDateList(sDate, eDate, timePer);
        }
        float dataResponse = dataStopwatch.ElapsedMilliseconds;
        dataStopwatch.Stop();
        visStopwatch.Start();
        mapSpawnner.PopulateData(response, DateList);
        visualisationStatus.text = "Visualisation ready";
        float visualTime = visStopwatch.ElapsedMilliseconds;
        visStopwatch.Stop();
        stopwatch.Stop();
        float totalTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        dataStopwatch.Reset();
        visStopwatch.Reset();
        var ramMB = memoryProfiler.ReturnMemoryReserved();
        //var ramMB = memoryProfiler.ReturnRAM();
        var totalRam = 65249.0f;
        var ramPerc = (float)((ramMB / totalRam) * 100);
        var cpuPerc = await memoryProfiler.ReturnCurrentProcessCPU();
        mapSpawnner.ClearVisualisationObjects();
        EvaluationTestModel testInfo = new EvaluationTestModel();
        if (DateList != null)
        {
            testInfo = new EvaluationTestModel
            {
                TotalTimeTaken = totalTime,
                DTResponseTimeTaken = dataResponse,
                VisualTimeTaken = visualTime,
                NumberOfDataPoints = DateList.Count,
                RAMusageMBTotal = ramMB,
                RAMusagePercTotal = ramPerc,
                CPUusageTotal = cpuPerc
            };
        }
        else
        {
            testInfo = new EvaluationTestModel
            {
                TotalTimeTaken = totalTime,
                DTResponseTimeTaken = dataResponse,
                VisualTimeTaken = visualTime,
                NumberOfDataPoints = 1,
                RAMusageMBTotal = ramMB,
                RAMusagePercTotal = ramPerc,
                CPUusageTotal = cpuPerc
            };
        }
        return testInfo;
    }
    #endregion
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
        if ((startDate.text != "") && (endDate.text != "") && (TimePeriod.value != 0))
        {
            DateSelectNextButton.interactable = true;
        }
    }
}
