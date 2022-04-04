using System;

namespace Building_DT
{
    class BuildingManager
    {
        private void InitialiseBuilding()
        {
            Building tempBuilding = new Building("test", 0, 0, null);
            tempBuilding.GetMeterLists();
        }
    }
}
