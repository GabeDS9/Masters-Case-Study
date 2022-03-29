using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    public EnergyAPIScript energyManager = new EnergyAPIScript();
    public List<EnergyMeterData> EnergyMeterList = new List<EnergyMeterData>();
    public MenuManager menuManager;

    // Start is called before the first frame update
    void Start()
    {
        EnergyMeterList = energyManager.LoadEnergyMeterList();
    }

    // Update is called once per frame
    void Update()
    {
        if(EnergyMeterList != null)
        {
            menuManager.EnergyMetersButton.SetActive(true);
        }
    }
}
