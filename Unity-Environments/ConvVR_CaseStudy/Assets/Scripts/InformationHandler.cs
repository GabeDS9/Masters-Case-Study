using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationHandler : MonoBehaviour
{
    private EnergyAPIScript energyCaller = new EnergyAPIScript();
    public string GetInformation(string DataType, string InformationType, string DisplayType, string ElementSelected, 
        List<string> ElementLevel, string startDate, string endDate, string timePeriod, List<ElementModel> CampusList,
        List<ElementModel> PrecinctList, List<ElementModel> BuildingList)
    {
        foreach(var level in ElementLevel)
        {
            if(level == "Building")
            {
                Debug.Log("Getting building info");
            }
            else if (level == "Precinct")
            {
                Debug.Log("Getting precinct info");
            }
            else if (level == "Campus")
            {
                Debug.Log("Getting campus info");
            }
        }
        return "";
    }

    private void GetBuildingInformation()
    {

    }
    private void GetPrecinctInformation()
    {

    }
    private void GetCampusInformation()
    {

    }
}
