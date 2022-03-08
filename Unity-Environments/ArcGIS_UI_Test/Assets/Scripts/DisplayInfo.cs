using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayInfo : MonoBehaviour
{
    /*[SerializeField]
    private GameObject cubeVis;
    [SerializeField]
    Mapbox.Unity.Map.AbstractMap map;
    /*[SerializeField]
    private TMP_Dropdown dropDown;*/

    private List<MeterList> meterList = new List<MeterList>();
    public Mapbox.Examples.SpawnOnMap spawnOnMap;

    public void PopulateMeters()
    {
        Debug.Log("Populating Meters");
        meterList = EnergyAPIScript.GetMeterList();
        Debug.Log("Meter list content: " + meterList.Count);
        if(meterList != null)
        {
            Debug.Log("Spawning Objects");
            //spawnOnMap.MarkerPrefab = cubeVis;
            //spawnOnMap.Map = map;
            spawnOnMap.PopulateObjects(meterList);
        }
    }

    /*public void DisplayCurrentEnergy()
    {
        fromDate = "2022-02-10%2009:25:00";
        currDate = "2022-02-10%2009:35:00";
        int pos = dropDown.value;
        int id = meterList[pos].meterid;
        string m_id = id.ToString();
        Debug.Log("Dropdown Meter ID: " + m_id);

        MeterData meterData = EnergyAPIScript.GetMeterData(fromDate, currDate, m_id, interval);
        List<Data> myData = meterData.data;

        float yScale = (float)myData[0].ptot_kw;

        Vector3 scaleChange = new Vector3(2f, yScale, 2f);
        Vector3 posTransform = new Vector3(xPos, yScale / 2, 0f);

        xPos += 4;

        GameObject vis = Instantiate(cubeVis);
        vis.transform.localScale = scaleChange;
        vis.transform.position = posTransform;
    }*/
}
