using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Utils;

public class InformationHandler : MonoBehaviour
{
    private EnergyAPIScript energyCaller = new EnergyAPIScript();
    private Utilities utilities = new Utilities();

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
        foreach (var level in ElementLevel)
        {
            if (level == "Building")
            {
                if (DisplayType == "Individual")
                {
                    var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, ElementSelected, startDate, endDate, timePeriod, BuildingList);
                    foreach (var build in BuildInfo)
                    {
                        BuildingDataList.Add(build);
                        dataList.Add(build);
                    }
                }
                else if (DisplayType == "Collective")
                {
                    foreach (var build in BuildingList)
                    {
                        if (build.ElementName == ElementSelected)
                        {
                            var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, ElementSelected, startDate, endDate, timePeriod, BuildingList);
                            foreach (var building in BuildInfo)
                            {
                                BuildingDataList.Add(building);
                                dataList.Add(building);
                            }
                        }
                    }
                    foreach (var prec in PrecinctList)
                    {
                        if (prec.ElementName == ElementSelected)
                        {
                            foreach (var build in prec.ChildElements)
                            {
                                var BuildInfo = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                                foreach (var building in BuildInfo)
                                {
                                    BuildingDataList.Add(building);
                                    dataList.Add(building);
                                }
                            }
                        }
                    }
                    foreach (var camp in CampusList)
                    {
                        if (camp.ElementName == ElementSelected)
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
                                            if (BuildInfo != null)
                                            {
                                                foreach (var building in BuildInfo)
                                                {
                                                    BuildingDataList.Add(building);
                                                    dataList.Add(building);
                                                }
                                            }
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
                    foreach (var precinct in PrecInfo)
                    {
                        PrecinctDataList.Add(precinct);
                        dataList.Add(precinct);
                    }
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
                                    foreach (var building in BuildInfo)
                                    {
                                        BuildingDataList.Add(building);
                                    }
                                }
                            }
                            var PrecInfo = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, ElementSelected, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                            foreach (var precinct in PrecInfo)
                            {
                                PrecinctDataList.Add(precinct);
                                dataList.Add(precinct);
                            }
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
                                                foreach (var building in BuildInfo)
                                                {
                                                    BuildingDataList.Add(building);
                                                }
                                            }
                                        }
                                        var PrecInfo = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec.ElementName, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                                        foreach (var precinct in PrecInfo)
                                        {
                                            PrecinctDataList.Add(precinct);
                                            dataList.Add(precinct);
                                        }
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
                    foreach (var camp in CampusInfo)
                    {
                        CampusDataList.Add(camp);
                        dataList.Add(camp);
                    }
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
                                                        foreach (var building in BuildInfo)
                                                        {
                                                            BuildingDataList.Add(building);
                                                        }
                                                    }
                                                }
                                                var PrecInfo = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec.ElementName, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                                                foreach (var precinct in PrecInfo)
                                                {
                                                    PrecinctDataList.Add(precinct);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var CampusInfo = await GetCampusInformationAsync(DataType, InformationType, DisplayType, ElementSelected, startDate, endDate, timePeriod, BuildingList, PrecinctList, CampusList);
                        foreach (var camp in CampusInfo)
                        {
                            CampusDataList.Add(camp);
                            dataList.Add(camp);
                        }
                    }
                }
            }
        }
        return dataList;
    }

    private async Task<List<DataModel>> GetBuildingInformationAsync(string DataType, string InformationType, string ElementSelected,
        string startDate, string endDate, string timePeriod, List<ElementModel> BuildingList)
    {
        List<DataModel> dataModels = new List<DataModel>();
        if (InformationType == "CurrentData")
        {
            double Ptot = 0;
            foreach (var build in BuildingList)
            {
                if (build.ElementName == ElementSelected)
                {
                    foreach (var meter in build.ChildElements)
                    {
                        var tempData = await energyCaller.GetCurrentEnergyDataAsync(int.Parse(meter));
                        if (tempData.Count > 0)
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
                    dataModels.Add(newData);
                    return dataModels;
                }
            }
            return null;
        }
        else if (InformationType == "Averages")
        {
            foreach (var build in BuildingList)
            {
                if (build.ElementName == ElementSelected)
                {
                    var dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
                    double[] averageData = new double[dateList.Count];
                    foreach (var meter in build.ChildElements)
                    {
                        var tempData = await energyCaller.CalculateAveragesAsync(int.Parse(meter), startDate, endDate, timePeriod);
                        while (tempData.Count != averageData.Length)
                        {
                            if (tempData.Count > 1)
                            {
                                tempData.Add(tempData[0]);
                            }
                            else
                            {
                                tempData.Add(0);
                            }
                        }
                        for (int i = 0; i < averageData.Length; i++)
                        {
                            if (tempData.Count > 0)
                            {
                                averageData[i] += tempData[i];
                            }
                            else
                            {
                                averageData[i] += 0;
                            }
                        }
                    }

                    for (int i = 0; i < averageData.Length; i++)
                    {
                        DataModel newData = new DataModel
                        {
                            DataType = DataType,
                            ElementType = "Building",
                            Element_Name = ElementSelected,
                            Latitude = build.Latitude,
                            Longitude = build.Longitude,
                            Meter_ID = 0,
                            Value = averageData[i],
                            Timestamp = dateList[i]
                        };
                        dataModels.Add(newData);
                    }

                    return dataModels;
                }
            }
            return null;
        }
        else if (InformationType == "Max")
        {
            foreach (var build in BuildingList)
            {
                if (build.ElementName == ElementSelected)
                {
                    var dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
                    double[] maxData = new double[dateList.Count];
                    foreach (var meter in build.ChildElements)
                    {
                        var tempData = await energyCaller.CalculateMaxesAsync(int.Parse(meter), startDate, endDate, timePeriod);
                        while (tempData.Count != maxData.Length)
                        {
                            if (tempData.Count > 1)
                            {
                                tempData.Add(tempData[0]);
                            }
                            else
                            {
                                tempData.Add(0);
                            }
                        }
                        for (int i = 0; i < maxData.Length; i++)
                        {
                            if (tempData.Count > 0)
                            {
                                maxData[i] += tempData[i];
                            }
                            else
                            {
                                maxData[i] += 0;
                            }
                        }
                    }

                    for (int i = 0; i < maxData.Length; i++)
                    {
                        DataModel newData = new DataModel
                        {
                            DataType = DataType,
                            ElementType = "Building",
                            Element_Name = ElementSelected,
                            Latitude = build.Latitude,
                            Longitude = build.Longitude,
                            Meter_ID = 0,
                            Value = maxData[i],
                            Timestamp = dateList[i]
                        };
                        dataModels.Add(newData);
                    }

                    return dataModels;
                }
            }
            return null;
        }
        else if (InformationType == "Total")
        {
            foreach (var build in BuildingList)
            {
                if (build.ElementName == ElementSelected)
                {
                    var dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
                    double[] totalData = new double[dateList.Count];
                    foreach (var meter in build.ChildElements)
                    {
                        var tempData = await energyCaller.CalculateTotalAsync(int.Parse(meter), startDate, endDate, timePeriod);
                        while (tempData.Count != totalData.Length)
                        {
                            if (tempData.Count > 1)
                            {
                                tempData.Add(tempData[0]);
                            }
                            else
                            {
                                tempData.Add(0);
                            }
                        }
                        for (int i = 0; i < totalData.Length; i++)
                        {
                            if (tempData.Count > 0)
                            {
                                totalData[i] += tempData[i];
                            }
                            else
                            {
                                totalData[i] += 0;
                            }
                        }
                    }

                    for (int i = 0; i < totalData.Length; i++)
                    {
                        DataModel newData = new DataModel
                        {
                            DataType = DataType,
                            ElementType = "Building",
                            Element_Name = ElementSelected,
                            Latitude = build.Latitude,
                            Longitude = build.Longitude,
                            Meter_ID = 0,
                            Value = totalData[i],
                            Timestamp = dateList[i]
                        };
                        dataModels.Add(newData);
                    }

                    return dataModels;
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }
    private async Task<List<DataModel>> GetPrecinctInformationAsync(string DataType, string InformationType, string DisplayType, string ElementSelected,
        string startDate, string endDate, string timePeriod, List<ElementModel> BuildingList, List<ElementModel> PrecinctList)
    {
        List<DataModel> dataModels = new List<DataModel>();
        if (InformationType == "CurrentData")
        {
            if (DisplayType == "Individual")
            {
                foreach (var prec in PrecinctList)
                {
                    double Ptot = 0;
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var build in prec.ChildElements)
                        {
                            var tempData = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                            Ptot += tempData[0].Value;
                        }
                        if (Ptot == 0)
                        {
                            var tempData = await energyCaller.GetCurrentEnergyDataAsync(int.Parse(prec.ChildElements[0]));
                            if (tempData.Count > 0)
                            {
                                Ptot += tempData[tempData.Count - 1].ptot_kw;
                            }
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
                        dataModels.Add(newData);
                        return dataModels;
                    }
                }
            }
            else if (DisplayType == "Collective")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        double Ptot = 0;
                        foreach (var data in BuildingDataList)
                        {
                            foreach (var build in prec.ChildElements)
                            {
                                if (build == data.Element_Name)
                                {
                                    Ptot += data.Value;
                                }
                            }
                        }
                        if (Ptot == 0)
                        {
                            var tempData = await energyCaller.GetCurrentEnergyDataAsync(int.Parse(prec.ChildElements[0]));
                            if (tempData.Count > 0)
                            {
                                Ptot += tempData[tempData.Count - 1].ptot_kw;
                            }
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
                        dataModels.Add(newData);
                        return dataModels;
                    }
                }
            }
            return null;
        }
        else if (InformationType == "Averages")
        {
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
            double[] averageData = new double[dateList.Count];
            bool isData = true;
            if (DisplayType == "Individual")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var build in prec.ChildElements)
                        {
                            var tempData = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                            for (int i = 0; i < averageData.Length; i++)
                            {
                                averageData[i] += tempData[i].Value;
                            }
                        }
                        if (averageData[0] == 0)
                        {
                            var temp = await energyCaller.CalculateAveragesAsync(int.Parse(prec.ChildElements[0]), startDate, endDate, timePeriod);
                            for (int i = 0; i < temp.Count; i++)
                            {
                                averageData[i] = temp[i];
                            }
                        }
                        for (int i = 0; i < averageData.Length; i++)
                        {
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Precinct",
                                Element_Name = ElementSelected,
                                Latitude = prec.Latitude,
                                Longitude = prec.Longitude,
                                Meter_ID = 0,
                                Value = averageData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            else if (DisplayType == "Collective")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var date in dateList)
                        {
                            double tempAvgPower = 0;
                            foreach (var build in prec.ChildElements)
                            {
                                foreach (var data in BuildingDataList)
                                {
                                    if (build == data.Element_Name)
                                    {
                                        if (date == data.Timestamp)
                                        {
                                            tempAvgPower += data.Value;
                                        }
                                    }
                                }
                            }
                            if (tempAvgPower == 0)
                            {
                                isData = false;
                                break;
                            }
                            if (isData)
                            {
                                DataModel newData = new DataModel
                                {
                                    DataType = DataType,
                                    ElementType = "Precinct",
                                    Element_Name = ElementSelected,
                                    Latitude = prec.Latitude,
                                    Longitude = prec.Longitude,
                                    Meter_ID = 0,
                                    Value = tempAvgPower,
                                    Timestamp = date
                                };
                                dataModels.Add(newData);
                            }
                        }                        
                    }
                    if (!isData)
                    {
                        var temp = await energyCaller.CalculateAveragesAsync(int.Parse(prec.ChildElements[0]), startDate, endDate, timePeriod);
                        for (int i = 0; i < temp.Count; i++)
                        {
                            averageData[i] = temp[i];
                        }
                        for (int i = 0; i < averageData.Length; i++)
                        {
                            DataModel newDat = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Precinct",
                                Element_Name = ElementSelected,
                                Latitude = prec.Latitude,
                                Longitude = prec.Longitude,
                                Meter_ID = 0,
                                Value = averageData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newDat);
                        }
                        isData = true;
                    }
                }
                return dataModels;
            }
            return null;
        }
        else if (InformationType == "Max")
        {
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
            double[] maxData = new double[dateList.Count];
            bool isData = true;
            if (DisplayType == "Individual")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var build in prec.ChildElements)
                        {
                            var tempData = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                            for (int i = 0; i < maxData.Length; i++)
                            {
                                maxData[i] += tempData[i].Value;
                            }
                        }
                        if (maxData[0] == 0)
                        {
                            var temp = await energyCaller.CalculateMaxesAsync(int.Parse(prec.ChildElements[0]), startDate, endDate, timePeriod);
                            for (int i = 0; i < temp.Count; i++)
                            {
                                maxData[i] = temp[i];
                            }
                        }
                        for (int i = 0; i < maxData.Length; i++)
                        {
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Precinct",
                                Element_Name = ElementSelected,
                                Latitude = prec.Latitude,
                                Longitude = prec.Longitude,
                                Meter_ID = 0,
                                Value = maxData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            else if (DisplayType == "Collective")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var date in dateList)
                        {
                            double tempMaxPower = 0;
                            foreach (var build in prec.ChildElements)
                            {
                                foreach (var data in BuildingDataList)
                                {
                                    if (build == data.Element_Name)
                                    {
                                        if (date == data.Timestamp)
                                        {
                                            tempMaxPower += data.Value;
                                        }
                                    }
                                }
                            }
                            if (tempMaxPower == 0)
                            {
                                isData = false;
                                break;
                            }
                            if (isData)
                            {
                                DataModel newData = new DataModel
                                {
                                    DataType = DataType,
                                    ElementType = "Precinct",
                                    Element_Name = ElementSelected,
                                    Latitude = prec.Latitude,
                                    Longitude = prec.Longitude,
                                    Meter_ID = 0,
                                    Value = tempMaxPower,
                                    Timestamp = date
                                };
                                dataModels.Add(newData);
                            }
                        }
                    }
                    if (!isData)
                    {
                        var temp = await energyCaller.CalculateMaxesAsync(int.Parse(prec.ChildElements[0]), startDate, endDate, timePeriod);
                        for (int i = 0; i < temp.Count; i++)
                        {
                            maxData[i] = temp[i];
                        }
                        for (int i = 0; i < maxData.Length; i++)
                        {
                            DataModel newDat = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Precinct",
                                Element_Name = ElementSelected,
                                Latitude = prec.Latitude,
                                Longitude = prec.Longitude,
                                Meter_ID = 0,
                                Value = maxData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newDat);
                        }
                        isData = true;
                    }
                }
                return dataModels;
            }
            return null;
        }
        else if (InformationType == "Total")
        {
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
            double[] totalData = new double[dateList.Count];
            bool isData = true;
            if (DisplayType == "Individual")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var build in prec.ChildElements)
                        {
                            var tempData = await GetBuildingInformationAsync(DataType, InformationType, build, startDate, endDate, timePeriod, BuildingList);
                            for (int i = 0; i < totalData.Length; i++)
                            {
                                totalData[i] += tempData[i].Value;
                            }
                        }
                        if (totalData[0] == 0)
                        {
                            var temp = await energyCaller.CalculateMaxesAsync(int.Parse(prec.ChildElements[0]), startDate, endDate, timePeriod);
                            for (int i = 0; i < temp.Count; i++)
                            {
                                totalData[i] = temp[i];
                            }
                        }
                        for (int i = 0; i < totalData.Length; i++)
                        {
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Precinct",
                                Element_Name = ElementSelected,
                                Latitude = prec.Latitude,
                                Longitude = prec.Longitude,
                                Meter_ID = 0,
                                Value = totalData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            else if (DisplayType == "Collective")
            {
                foreach (var prec in PrecinctList)
                {
                    if (prec.ElementName == ElementSelected)
                    {
                        foreach (var date in dateList)
                        {
                            double tempTotalPower = 0;
                            foreach (var build in prec.ChildElements)
                            {
                                foreach (var data in BuildingDataList)
                                {
                                    if (build == data.Element_Name)
                                    {
                                        if (date == data.Timestamp)
                                        {
                                            tempTotalPower += data.Value;
                                        }
                                    }
                                }
                            }
                            if (tempTotalPower == 0)
                            {
                                isData = false;
                                break;
                            }
                            if (isData)
                            {
                                DataModel newData = new DataModel
                                {
                                    DataType = DataType,
                                    ElementType = "Precinct",
                                    Element_Name = ElementSelected,
                                    Latitude = prec.Latitude,
                                    Longitude = prec.Longitude,
                                    Meter_ID = 0,
                                    Value = tempTotalPower,
                                    Timestamp = date
                                };
                                dataModels.Add(newData);
                            }
                        }
                    }
                    if (!isData)
                    {
                        var temp = await energyCaller.CalculateTotalAsync(int.Parse(prec.ChildElements[0]), startDate, endDate, timePeriod);
                        for (int i = 0; i < temp.Count; i++)
                        {
                            totalData[i] = temp[i];
                        }
                        for (int i = 0; i < totalData.Length; i++)
                        {
                            DataModel newDat = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Precinct",
                                Element_Name = ElementSelected,
                                Latitude = prec.Latitude,
                                Longitude = prec.Longitude,
                                Meter_ID = 0,
                                Value = totalData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newDat);
                        }
                        isData = true;
                    }
                }
                return dataModels;
            }
            return null;
        }
        else
        {
            return null;
        }
    }
    private async Task<List<DataModel>> GetCampusInformationAsync(string DataType, string InformationType, string DisplayType, string ElementSelected,
        string startDate, string endDate, string timePeriod, List<ElementModel> BuildingList, List<ElementModel> PrecinctList, List<ElementModel> CampusList)
    {
        List<DataModel> dataModels = new List<DataModel>();
        if (InformationType == "CurrentData")
        {
            if (DisplayType == "Individual")
            {
                foreach (var campus in CampusList)
                {
                    double Ptot = 0;
                    if (campus.ElementName == ElementSelected)
                    {
                        foreach (var prec in campus.ChildElements)
                        {
                            var tempData = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                            Ptot += tempData[0].Value;
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
                        dataModels.Add(newData);
                        return dataModels;
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
                            foreach (var prec in camp.ChildElements)
                            {
                                if (prec == data.Element_Name)
                                {
                                    Ptot += data.Value;
                                }
                            }
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
                        dataModels.Add(newData);
                        return dataModels;
                    }
                }
                return null;
            }
            return null;
        }
        else if (InformationType == "Averages")
        {
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
            double[] averageData = new double[dateList.Count];
            if (DisplayType == "Individual")
            {
                foreach (var camp in CampusList)
                {
                    if (camp.ElementName == ElementSelected)
                    {
                        foreach (var data in PrecinctDataList)
                        {
                            foreach (var prec in camp.ChildElements)
                            {
                                if (prec == data.Element_Name)
                                {
                                    var tempData = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                                    for (int i = 0; i < averageData.Length; i++)
                                    {
                                        averageData[i] += tempData[i].Value;
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < averageData.Length; i++)
                        {
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Campus",
                                Element_Name = ElementSelected,
                                Latitude = camp.Latitude,
                                Longitude = camp.Longitude,
                                Meter_ID = 0,
                                Value = averageData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            else if (DisplayType == "Collective")
            {
                foreach (var camp in CampusList)
                {
                    if (camp.ElementName == ElementSelected)
                    {
                        foreach (var date in dateList)
                        {
                            double tempAvgPower = 0;
                            foreach (var data in PrecinctDataList)
                            {
                                foreach (var prec in camp.ChildElements)
                                {
                                    if (prec == data.Element_Name)
                                    {
                                        if (date == data.Timestamp)
                                        {
                                            tempAvgPower += data.Value;
                                        }
                                    }
                                }
                            }
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Campus",
                                Element_Name = ElementSelected,
                                Latitude = camp.Latitude,
                                Longitude = camp.Longitude,
                                Meter_ID = 0,
                                Value = tempAvgPower,
                                Timestamp = date
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            return null;
        }
        else if (InformationType == "Max")
        {
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
            double[] maxData = new double[dateList.Count];
            if (DisplayType == "Individual")
            {
                foreach (var camp in CampusList)
                {
                    if (camp.ElementName == ElementSelected)
                    {
                        foreach (var data in PrecinctDataList)
                        {
                            foreach (var prec in camp.ChildElements)
                            {
                                if (prec == data.Element_Name)
                                {
                                    var tempData = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                                    for (int i = 0; i < maxData.Length; i++)
                                    {
                                        maxData[i] += tempData[i].Value;
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < maxData.Length; i++)
                        {
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Campus",
                                Element_Name = ElementSelected,
                                Latitude = camp.Latitude,
                                Longitude = camp.Longitude,
                                Meter_ID = 0,
                                Value = maxData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            else if (DisplayType == "Collective")
            {
                foreach (var camp in CampusList)
                {
                    if (camp.ElementName == ElementSelected)
                    {
                        foreach (var date in dateList)
                        {
                            double tempMaxPower = 0;
                            foreach (var data in PrecinctDataList)
                            {
                                foreach (var prec in camp.ChildElements)
                                {
                                    if (prec == data.Element_Name)
                                    {
                                        if (date == data.Timestamp)
                                        {
                                            tempMaxPower += data.Value;
                                        }
                                    }
                                }
                            }
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Campus",
                                Element_Name = ElementSelected,
                                Latitude = camp.Latitude,
                                Longitude = camp.Longitude,
                                Meter_ID = 0,
                                Value = tempMaxPower,
                                Timestamp = date
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            return null;
        }
        else if (InformationType == "Total")
        {
            List<string> dateList = utilities.GenerateDateList(startDate, endDate, timePeriod);
            double[] totalData = new double[dateList.Count];
            if (DisplayType == "Individual")
            {
                foreach (var camp in CampusList)
                {
                    if (camp.ElementName == ElementSelected)
                    {
                        foreach (var data in PrecinctDataList)
                        {
                            foreach (var prec in camp.ChildElements)
                            {
                                if (prec == data.Element_Name)
                                {
                                    var tempData = await GetPrecinctInformationAsync(DataType, InformationType, DisplayType, prec, startDate, endDate, timePeriod, BuildingList, PrecinctList);
                                    for (int i = 0; i < totalData.Length; i++)
                                    {
                                        totalData[i] += tempData[i].Value;
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < totalData.Length; i++)
                        {
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Campus",
                                Element_Name = ElementSelected,
                                Latitude = camp.Latitude,
                                Longitude = camp.Longitude,
                                Meter_ID = 0,
                                Value = totalData[i],
                                Timestamp = dateList[i]
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            else if (DisplayType == "Collective")
            {
                foreach (var camp in CampusList)
                {
                    if (camp.ElementName == ElementSelected)
                    {
                        foreach (var date in dateList)
                        {
                            double tempTotalPower = 0;
                            foreach (var data in PrecinctDataList)
                            {
                                foreach (var prec in camp.ChildElements)
                                {
                                    if (prec == data.Element_Name)
                                    {
                                        if (date == data.Timestamp)
                                        {
                                            tempTotalPower += data.Value;
                                        }
                                    }
                                }
                            }
                            DataModel newData = new DataModel
                            {
                                DataType = DataType,
                                ElementType = "Campus",
                                Element_Name = ElementSelected,
                                Latitude = camp.Latitude,
                                Longitude = camp.Longitude,
                                Meter_ID = 0,
                                Value = tempTotalPower,
                                Timestamp = date
                            };
                            dataModels.Add(newData);
                        }
                    }
                }
                return dataModels;
            }
            return null;
        }
        else
        {
            return null;
        }
    }
}
