using System;
using System.Collections.Generic;

namespace Building_DT
{
    public class BuildingManager
    {
        public List<Building> InitialiseBuildings(string precinct_name)
        {
            LoadExcel excel = new LoadExcel();
            List<Building> buildingList = new List<Building>();
            buildingList = excel.LoadBuildingData(precinct_name);
            
            return buildingList;
        }
    }
}
