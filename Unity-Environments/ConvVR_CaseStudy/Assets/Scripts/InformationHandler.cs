using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class InformationHandler : MonoBehaviour
{
    private EnergyAPIScript energyCaller = new EnergyAPIScript();
    private List<DataModel> BuildingDataList = new List<DataModel>();
    private List<DataModel> PrecinctDataList = new List<DataModel>();
    private List<DataModel> CampusDataList = new List<DataModel>();
    public async Task<List<DataModel>> GetInformationAsync(string DataType, string InformationType, string DisplayType, string ElementSelected, 
        List<string> ElementLevel, string startDate, string endDate, string timePeriod, List<ElementModel> CampusList,
        List<ElementModel> PrecinctList, List<ElementModel> BuildingList)
    {
        List<DataModel> dataList = new List<DataModel>();
        BuildingDataList.Clear();
        PrecinctDataList.Clear();
        CampusDataList.Clear();
        dataList.Clear();
        foreach(var level in ElementLevel)
        {
            if (level == "Building")
            {
                if (DisplayType == "Individual")
                {
                    var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, ElementSelected, startDate, endDate, timePeriod, BuildingList);
                    BuildingDataList.Add(BuildInfo);
                    dataList.Add(BuildInfo);
                }
                else if (DisplayType == "Collective")
                {
                    foreach(var build in BuildingList)
                    {
                        if(build.ElementName == ElementSelected)
                        {
                            var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, ElementSelected, startDate, endDate, timePeriod, BuildingList);
                            BuildingDataList.Add(BuildInfo);
                            dataList.Add(BuildInfo);
                        }
                    }
                    foreach (var prec in PrecinctList)
                    {
                        if (prec.ElementName == ElementSelected)
                        {
                            foreach (var build in prec.ChildElements)
                            {
                                var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                                BuildingDataList.Add(BuildInfo);
                                dataList.Add(BuildInfo);
                            }
                        }
                    }
                    foreach(var camp in CampusList)
                    {
                        if(camp.ElementName == ElementSelected)
                        {
                            foreach (var childPrec in camp.ChildElements)
                            {
                                foreach (var prec in PrecinctList)
                                {
                                    if (prec.ElementName == childPrec)
                                    {
                                        foreach (var build in prec.ChildElements)
                                        {
                                            var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                                            BuildingDataList.Add(BuildInfo);
                                            dataList.Add(BuildInfo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (level == "Precinct")
            {
                if (DisplayType == "Individual")
                {
                    var PrecInfo = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, ElementSelected, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                    PrecinctDataList.Add(PrecInfo);
                    dataList.Add(PrecInfo);
                }
                else if (DisplayType == "Collective")
                {
                    foreach (var prec in PrecinctList)
                    {
                        if (prec.ElementName == ElementSelected)
                        {
                            if (BuildingDataList.Count < 1)
                            {
                                foreach (var build in prec.ChildElements)
                                {
                                    var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                                    BuildingDataList.Add(BuildInfo);
                                }
                            }
                            var PrecInfo = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, ElementSelected, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                            PrecinctDataList.Add(PrecInfo);
                            dataList.Add(PrecInfo);
                        }
                    }
                    foreach (var campus in CampusList)
                    {
                        if (campus.ElementName == ElementSelected)
                        {
                            foreach (var childPrec in campus.ChildElements)
                            {
                                foreach (var prec in PrecinctList)
                                {
                                    if (prec.ElementName == childPrec)
                                    {
                                        if (BuildingDataList.Count < 1)
                                        {
                                            foreach (var build in prec.ChildElements)
                                            {
                                                var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                                                BuildingDataList.Add(BuildInfo);
                                            }
                                        }
                                        var PrecInfo = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec.ElementName, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                                        PrecinctDataList.Add(PrecInfo);
                                        dataList.Add(PrecInfo);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (level == "Campus")
            {
                if (DisplayType == "Individual")
                {
                    var CampusInfo = await GetCampusInformationAsync(DataType, InformationType, DisplayType, ElementSelected, startDate, endDate, timePeriod, BuildingList, PrecinctList, CampusList);
                    CampusDataList.Add(CampusInfo);
                    dataList.Add(CampusInfo);
                }
                else if (DisplayType == "Collective")
                {
                    foreach (var campus in CampusList)
                    {
                        if (PrecinctDataList.Count == 0)
                        {
                            if (campus.ElementName == ElementSelected)
                            {
                                foreach (var childPrec in campus.ChildElements)
                                {
                                    foreach (var prec in PrecinctList)
                                    {
                                        if (prec.ElementName == childPrec)
                                        {
                                            if (PrecinctDataList.Count < 1)
                                            {
                                                if (BuildingDataList.Count < 1)
                                                {
                                                    foreach (var build in prec.ChildElements)
                                                    {
                                                        var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                                                        BuildingDataList.Add(BuildInfo);
                                                    }
                                                }
                                                var PrecInfo = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec.ElementName, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                                                PrecinctDataList.Add(PrecInfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var CampusInfo = await GetCampusInformationAsync(DataType, InformationType, DisplayType, ElementSelected, startDate, endDate, timePeriod, BuildingList, PrecinctList, CampusList);
                        CampusDataList.Add(CampusInfo);
                        dataList.Add(CampusInfo);
                    }
                }
            }
        }        
        return dataList;
    }

    private async Task<DataModel> GetBuildingInformationAsync(string DataType, string InformationType, string ElementSelected, 
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
    private async Task<DataModel> GetPrecinctInformationAsync(string DataType, string InformationType, string DisplayType, string ElementSelected,
        string startDate, string endDate, string timePeriod, List<ElementModel> BuildingList, List<ElementModel> PrecinctList)
    {
        if (InformationType == "CurrentData")
        {
            if(DisplayType == "Individual")
            {
                double Ptot = 0;
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var build in prec.ChildElements)
                        {
                            var tempData = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                            Ptot += tempData.Value;
                        }
                        DataModel newData = new DataModel
                        {
                            DataType = DataType,
                            ElementType = "Precinct",
                            Element_Name = ElementSelected,
                            Latitude = prec.Latitude,
                            Longitude = prec.Longitude,
                            Meter_ID = 0,
                            Value = Ptot,
                            Timestamp = "Latest Reading"
                        };
                        return newData;
                    }
                }
            }
            else if(DisplayType == "Collective")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        double Ptot = 0;
                        foreach (var data in BuildingDataList)
                        {
                            Ptot += data.Value;
                        }
                        DataModel newData = new DataModel
                        {
                            DataType = DataType,
                            ElementType = "Precinct",
                            Element_Name = ElementSelected,
                            Latitude = prec.Latitude,
                            Longitude = prec.Longitude,
                            Meter_ID = 0,
                            Value = Ptot,
                            Timestamp = "Latest Reading"
                        };
                        return newData;
                    }
                }
                        return null;
            }
            return null;
        }
        else
        {
            return null;
        }
    }
    private async Task<DataModel> GetCampusInformationAsync(string DataType, string InformationType, string DisplayType, string ElementSelected,
        string startDate, string endDate, string timePeriod, List<ElementModel> BuildingList, List<ElementModel> PrecinctList, List<ElementModel> CampusList)
    {
        if (InformationType == "CurrentData")
        {
            if (DisplayType == "Individual")
            {
                double Ptot = 0;
                foreach (var campus in CampusList)
                {
                    if (campus.ElementName == ElementSelected)
                    {
                        foreach (var prec in campus.ChildElements)
                        {
                            var tempData = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                            Ptot += tempData.Value;
                        }
                        DataModel newData = new DataModel
                        {
                            DataType = DataType,
                            ElementType = "Campus",
                            Element_Name = ElementSelected,
                            Latitude = campus.Latitude,
                            Longitude = campus.Longitude,
                            Meter_ID = 0,
                            Value = Ptot,
                            Timestamp = "Latest Reading"
                        };
                        return newData;
                    }
                }
            }
            else if (DisplayType == "Collective")
            {
                foreach (var camp in CampusList)
                {
                    if (camp.ElementName == ElementSelected)
                    {
                        double Ptot = 0;
                        foreach (var data in PrecinctDataList)
                        {
                            Ptot += data.Value;
                        }
                        DataModel newData = new DataModel
                        {
                            DataType = DataType,
                            ElementType = "Campus",
                            Element_Name = ElementSelected,
                            Latitude = camp.Latitude,
                            Longitude = camp.Longitude,
                            Meter_ID = 0,
                            Value = Ptot,
                            Timestamp = "Latest Reading"
                        };
                        return newData;
                    }
                }
                return null;
            }
            return null;
        }
        else
        {
            return null;
        }
    }
}
