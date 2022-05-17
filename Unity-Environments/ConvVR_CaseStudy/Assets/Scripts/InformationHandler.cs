using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class InformationHandler : MonoBehaviour
{
    private EnergyAPIScript energyCaller = new EnergyAPIScript();
    private List<DataModel> buildingDataList = new List<DataModel>();
    private List<DataModel> precinctDataList = new List<DataModel>();
    private List<DataModel> CampusDataList = new List<DataModel>();
    public async Task<List<DataModel>> GetInformationAsync(string DataType, string InformationType, string DisplayType, string ElementSelected, 
        List<string> ElementLevel, string startDate, string endDate, string timePeriod, List<ElementModel> CampusList,
        List<ElementModel> PrecinctList, List<ElementModel> BuildingList)
    {
        List<DataModel> dataList = new List<DataModel>();

        foreach(var level in ElementLevel)
        {
            if(level == "Building")
            {
                var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, DisplayType, ElementSelected, startDate, endDate, timePeriod, BuildingList);
                buildingDataList.Add(BuildInfo);
                dataList.Add(BuildInfo);
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
        
        return dataList;
    }

    private async Task<DataModel> GetBuildingInformationAsync(string DataType, string InformationType, string DisplayType, string ElementSelected, 
        string startDate, string endDate, string timePeriod, List<ElementModel> BuildingList)
    {
        if(InformationType == "CurrentData")
        {
            double Ptot = 0;
            foreach(var build in BuildingList)
            {
                if(build.ElementName == ElementSelected)
                {
                    foreach(var meter in build.ChildElements)
                    {
                        var tempData = await energyCaller.GetCurrentEnergyDataAsync(int.Parse(meter));
                        if(tempData.Count > 0)
                        {
                            Ptot += tempData[tempData.Count - 1].ptot_kw;
                        }
                    }
                    DataModel newData = new DataModel
                    {
                        DataType = DataType,
                        ElementType = "Building",
                        Element_Name = ElementSelected,
                        Latitude = build.Latitude,
                        Longitude = build.Longitude,
                        Meter_ID = 0,
                        Value = Ptot,
                        Timestamp = "Latest Reading"
                    };
                    return newData;
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }
    private void GetPrecinctInformation()
    {

    }
    private void GetCampusInformation()
    {

    }
}
