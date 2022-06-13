using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Models;
//using CsvHelper;
//using CsvHelper.Configuration;
using System.Globalization;
using Unity;
using UnityEngine;

public class WriteToCSVFile : MonoBehaviour
{
    static string filename = "LatencyEvaluation";
    string filepath = filename;
    public void addRecords(List<EvaluationTestModel> testInfoList)
    {
        try
        {
            Debug.Log(testInfoList.Count);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Could not write to CSV file :", ex);
        }
        /*try
        {
            var configPersons = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using (var stream = File.Open(filepath, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, configPersons))
            {
                csv.WriteRecords(testInfoList);
            }
        }
        catch(Exception ex)
        {
            throw new ApplicationException("Could not write to CSV file :", ex);
        }*/
    }
}
