using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Models;
using System.Management;
using System.Linq;

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

    // Evaluation parameters
    Stopwatch stopwatch = new Stopwatch();
    Stopwatch dtStopwatch = new Stopwatch();
    Stopwatch visStopwatch = new Stopwatch();
    public CSVWriter csvWriter;
    public MemoryProfiler memoryProfiler;
    public Test tester;
    /*PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    PerformanceCounter cpuUnityCounter = new PerformanceCounter("Process", "% Processor Time", "Unity Editor", true);
    PerformanceCounter cpuDTCounter = new PerformanceCounter("Process", "% Processor Time", "Campus_DT", true);
    PerformanceCounter cpuServicesCounter = new PerformanceCounter("Process", "% Processor Time", "DT_Services", true);*/

    // Start is called before the first frame update
    void Start()
    {
        myClient.LoadServiceGateway();
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
        //DisplayMainMenu();
        _ = RunEvaluationTest();
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
        MainMenu.SetActive(false);        
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
        if (StartDateSelected != "" && EndDateSelected != "" && TimePeriodSelected != "")
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
            PlayUIButton.GetComponentInChildren<Text>().text = "Pause";
            InvokeRepeating("PlayFunction", 0.2f, 0.2f);
        }
        else if (PlayUIButton.GetComponentInChildren<Text>().text == "Pause")
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
        var response = await myClient.sendMessageAsync(message);
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
        var DateList = utils.GenerateDateList(StartDateSelected, EndDateSelected, TimePeriodSelected);
        var response = await myClient.sendMessageAsync(message);        
        mapSpawnner.PopulateData(response, DateList);
        visualisationStatus.text = "Visualisation ready";
    }

    #region EvaluationFunctions
    public async Task RunEvaluationTest()
    {
        List<EvaluationTestModel> testInformation = new List<EvaluationTestModel>();
        
        int numberOfDTs = 0;
        int numberOfCampuses = 0;
        int numberOfPrecincts = 0;
        int numberOfBuildings = 0;
        var campusDTList = await messHandler.GetDTListAsync("");
        numberOfDTs += campusDTList.Count;
        numberOfCampuses += campusDTList.Count;
        foreach (var campus in campusDTList)
        {
            var precinctDTList = await messHandler.GetDTListAsync(campus);
            numberOfDTs += precinctDTList.Count;
            numberOfPrecincts += precinctDTList.Count;
            foreach (var prec in precinctDTList)
            {
                var buildingDTList = await messHandler.GetDTListAsync(prec);
                numberOfDTs += buildingDTList.Count;
                numberOfBuildings += buildingDTList.Count;
            }
        }

        string sDate = "2022-5-1";
        string eDate = "2022-5-30";
        var DateList = utils.GenerateDateList(sDate, eDate, "Day");
        List<string> dtLevel = new List<string>();
        dtLevel.Add("All");
        foreach(var date in DateList)
        {
            EvaluationTestModel temp = await GetInformation("Energy", "Averages", "Collective", "Stellenbosch University", dtLevel, sDate, date, "Day");
            EvaluationTestModel testInfo = new EvaluationTestModel { NumberOfDTs = numberOfDTs, NumberOfCampuses = numberOfCampuses, 
                NumberOfPrecincts = numberOfPrecincts, NumberOfBuildings = numberOfBuildings, TotalTimeTaken = temp.TotalTimeTaken, 
                DTResponseTimeTaken = temp.DTResponseTimeTaken, VisualTimeTaken = temp.VisualTimeTaken, NumberOfDataPoints = temp.NumberOfDataPoints, 
                RAMusageMBUnity = temp.RAMusageMBUnity, RAMusageMBDTs = temp.RAMusageMBDTs, RAMusageMBServices = temp.RAMusageMBServices,
                RAMusagePercUnity = temp.RAMusagePercUnity, RAMusagePercDTs = temp.RAMusagePercDTs, RAMusagePercServices = temp.RAMusagePercServices,
                RAMusageMBTotal = temp.RAMusageMBTotal, RAMusagePercTotal = temp.RAMusagePercTotal, CPUusageDTs = temp.CPUusageDTs,
                CPUusageServices = temp.CPUusageServices, CPUusageUnity = temp.CPUusageUnity, CPUusageTotal = temp.CPUusageTotal
            };
            testInformation.Add(testInfo);
        }
        visualisationStatus.text = "Test Complete";
        csvWriter.WriteCSV(testInformation);
    }
    private async Task<EvaluationTestModel> GetInformation(string dataType, string informationType, string displayType, string digitalTwin, List<string> dtLevel,
        string sDate, string eDate, string timePer)
    {
        stopwatch.Start();
        dtStopwatch.Start();
        visualisationStatus.text = "Getting visualisation ready...";
        var message = CreateMessage(dataType, informationType, displayType, digitalTwin, dtLevel, sDate, eDate, timePer);
        var DateList = utils.GenerateDateList(sDate, eDate, timePer);
        var response = await myClient.sendMessageAsync(message);
        float dtResponse = dtStopwatch.ElapsedMilliseconds;
        dtStopwatch.Stop();
        visStopwatch.Start();
        mapSpawnner.PopulateData(response, DateList);        
        visualisationStatus.text = "Visualisation ready";
        float visualTime = visStopwatch.ElapsedMilliseconds;
        visStopwatch.Stop();
        stopwatch.Stop();
        float totalTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        dtStopwatch.Reset();
        visStopwatch.Reset();
        //var memoryUsageMB = memoryProfiler.ReturnMemoryReserved();//CalculateRAMUsageMB();
        //var memoryUsagePerc = ((float)memoryUsageMB / 65249.0f) * 100.0f;
        //var cpuUsage = cpuUnityCounter.NextValue();
        //var temp = "";
        MessageModel mes = new MessageModel { ServiceTag = "Usages" };
        string temp = JsonConvert.SerializeObject(mes);
        var resp = await myClient.sendMessageAsync(temp);
        var totalRam = 65249.0f;
        //var ram = tester.ReturnRAMUsages();
        var ram = JsonConvert.DeserializeObject<List<float>>(resp);
        
        var memoryUsageMBDTs = ram[0];
        var memoryUsagePercDTs = (float)((memoryUsageMBDTs / totalRam)*100);
        var cpuUsageDTs = ram[1];
        var memoryUsageMBService = ram[2];
        var memoryUsagePercService = (float)((memoryUsageMBService / totalRam) * 100);
        var cpuUsageService = ram[3];
        var memoryUsageMBUnity = memoryProfiler.ReturnMemoryReserved(); ;
        var memoryUsagePercUnity = (float)((memoryUsageMBUnity / totalRam) * 100);
        var cpuUsageUnity = ram[5];
        var ramTotalMB = memoryUsageMBDTs + memoryUsageMBService + memoryUsageMBUnity;
        var ramTotalPerc = memoryUsagePercDTs + memoryUsagePercService + memoryUsagePercUnity;
        var cpuTotal = cpuUsageDTs + cpuUsageService + cpuUsageUnity;
        mapSpawnner.ClearVisualisationObjects();
        EvaluationTestModel testInfo = new EvaluationTestModel { TotalTimeTaken = totalTime, DTResponseTimeTaken = dtResponse, 
            VisualTimeTaken = visualTime, NumberOfDataPoints = DateList.Count, RAMusageMBUnity = memoryUsageMBUnity,
            RAMusageMBDTs = memoryUsageMBDTs, RAMusageMBServices = memoryUsageMBService, RAMusageMBTotal = ramTotalMB,
            RAMusagePercUnity = memoryUsagePercUnity, RAMusagePercDTs = memoryUsagePercDTs, RAMusagePercServices = memoryUsagePercService, 
            RAMusagePercTotal = ramTotalPerc, CPUusageDTs = cpuUsageDTs, CPUusageServices = cpuUsageService, CPUusageUnity = cpuUsageUnity,
            CPUusageTotal = cpuTotal
        };
        return testInfo;
    }
    private string CreateMessage(string dataType, string informationType, string displayType, string digitalTwin, List<string> dtLevel,
        string sDate, string eDate, string timePer)
    {
        var message = new MessageModel
        {
            ServiceTag = "Exploratory",
            DataType = dataType,
            InformationType = informationType,
            DisplayType = displayType,
            DigitalTwin = digitalTwin,
            DTDetailLevel = dtLevel,
            startDate = sDate,
            endDate = eDate,
            timePeriod = timePer
        };
        var mes = JsonConvert.SerializeObject(message);
        return mes;
    }
    private double CalculateCPUUsagePerc()
    {
        double usage = 0;
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
        foreach (ManagementObject obj in searcher.Get())
        {
            var value = obj["PercentProcessorTime"];
            usage = double.Parse(value.ToString());
            var name = obj["Name"];
            Console.WriteLine(name + " : " + usage);
        }
        return usage;
    }
    private double CalculateRAMUsagePerc()
    {
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

        var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
            FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
            TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
        }).FirstOrDefault();

        double percent = 0;
        if (memoryValues != null)
        {
            percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
        }
        return percent;
    }
    private double CalculateRAMUsageMB()
    {
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

        var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
            FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
            TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
        }).FirstOrDefault();

        double usage = 0;
        if (memoryValues != null)
        {
            usage = (memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory);
        }

        return usage;
    }
    #endregion
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
    }    public void ChangeDisplayLevelNext()
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
}
