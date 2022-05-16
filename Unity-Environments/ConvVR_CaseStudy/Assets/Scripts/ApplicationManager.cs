using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    public EnergyAPIScript energyManager = new EnergyAPIScript();
    public List<EnergyMeterData> EnergyMeterList = new List<EnergyMeterData>();
    public OccupancyAPIScript occupancyManager = new OccupancyAPIScript();
    public List<OccupancyMeterData> OccupancyMeterList = new List<OccupancyMeterData>();
    public SolarAPIScript solarManager = new SolarAPIScript();
    public List<SolarMeterData> SolarMeterList = new List<SolarMeterData>();
    public MenuManager menuManager;

    // Start is called before the first frame update
    void Start()
    {
        /*EnergyMeterList = energyManager.LoadEnergyMeterList();
        OccupancyMeterList = occupancyManager.LoadOccupancyMeterList();
        SolarMeterList = solarManager.LoadSolarMeterList();*/
    }

    // Update is called once per frame
    /*void Update()
    {
        if (EnergyMeterList != null)
        {
            menuManager.EnergyMetersButton.SetActive(true);
        }

        if (OccupancyMeterList != null)
        {
            menuManager.OccupancyMetersButton.SetActive(true);
        }

        if (SolarMeterList != null)
        {
            menuManager.SolarMetersButton.SetActive(true);
        }
    }*/
}
