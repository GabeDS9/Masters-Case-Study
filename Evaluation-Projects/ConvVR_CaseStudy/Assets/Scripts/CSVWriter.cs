using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Models;

public class CSVWriter : MonoBehaviour
{
    string filename = "";
    // Start is called before the first frame update
    void Start()
    {
        filename = Application.dataPath + "/LatencyEvaluation.csv";
    }

    public void WriteCSV(List<EvaluationTestModel> testInfoList)
    {
        if (testInfoList.Count > 0)
        {
            using (var stream = File.Open(filename, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var item in testInfoList)
                {
                    writer.WriteLine($"{item.NumberOfDTs},{item.NumberOfCampuses},{item.NumberOfPrecincts},{item.NumberOfBuildings}," +
                    $"{item.NumberOfDataPoints},{item.TotalTimeTaken},{item.DTResponseTimeTaken},{item.VisualTimeTaken}," +
                    $"{item.RAMusageMBTotal.ToString().Replace(',', '.')},{item.RAMusagePercTotal.ToString().Replace(',', '.')}," +
                    $"{item.CPUusageTotal.ToString().Replace(',', '.')}");
                }
                writer.Close();
            }
        }
    }
}
