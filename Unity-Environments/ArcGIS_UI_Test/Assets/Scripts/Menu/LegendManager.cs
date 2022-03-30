using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LegendManager : MonoBehaviour
{
    public List<SpawnedObject> spawnedEnergyObjects = new List<SpawnedObject>();
    public GameObject legendPanel;
    public bool isDisplayedPanel = false;

    private void Start()
    {
        legendPanel.SetActive(false);
    }
    public void DisplayLegendPanel()
    {
        if (isDisplayedPanel)
        {
            legendPanel.SetActive(false);
            isDisplayedPanel = false;
        }
        else if (!isDisplayedPanel)
        {
            legendPanel.SetActive(true);
            isDisplayedPanel = true;
        }
        
    }

    public void ManageEnergyObjects(EnergyMeterData energyMeter, string visualisation_type, GameObject instance, string manageType)
    {
        if(manageType == "add")
        {
            SpawnedObject tempObject = new SpawnedObject();
            tempObject.meterid = energyMeter.meterid;
            tempObject.visual = instance;
            tempObject.visualisation_type = visualisation_type;
            spawnedEnergyObjects.Add(tempObject);
        }
    }

}

public class SpawnedObject
{
    public string visualisation_type { get; set; }

    public int meterid { get; set; }

    public GameObject visual { get; set; }
}
