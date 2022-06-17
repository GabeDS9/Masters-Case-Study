using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DataModels;
using MongoDB;
using DataAccess;

public class LocalStorage : MonoBehaviour
{
    private List<EnergyMeter> meterList = new List<EnergyMeter>();
    LoadExcel excel = new LoadExcel();
    DBDataAccess db = new DBDataAccess();
    private EnergyAPIScript energyCaller = new EnergyAPIScript();
    private string startDate = "2022-05-01";
    private string endDate = "2022-05-05";

    private void Start()
    {
        //StoreMeterDataAsync();
    }
    private async void StoreMeterDataAsync()
    {
        db.DeleteDatabase("Conv_Energy_Data");
        meterList = excel.LoadMeters();
        foreach (var meter in meterList)
        {
            var tempData = await energyCaller.GetMeterDataAsync(startDate, endDate, int.Parse(meter.MeterID));
            foreach(var data in tempData)
            {
                var timestamp = data.timestamp.Split(' ');
                EnergyMeterModel tempMeter = new EnergyMeterModel(meter.MeterName, int.Parse(meter.MeterID), data.ptot_kw, data.difference_imp_kwh, timestamp[0], timestamp[1]);
                await db.CreateEnergyReading(tempMeter);
            }
            Debug.Log($"Energy Meter Data Stored for {meter.MeterID}");
        }
        Debug.Log("Energy Meter Data Stored");
    }
}
