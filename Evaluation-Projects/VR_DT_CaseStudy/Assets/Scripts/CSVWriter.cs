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
                    $"{item.RAMusageMBUnity.ToString().Replace(',', '.')},{item.RAMusageMBDTs.ToString().Replace(',', '.')}," +
                    $"{item.RAMusageMBServices.ToString().Replace(',', '.')},{item.RAMusageMBTotal.ToString().Replace(',', '.')}," +
                    $"{item.RAMusagePercUnity.ToString().Replace(',','.')},{item.RAMusagePercDTs.ToString().Replace(',', '.')}," +
                    $"{item.RAMusagePercServices.ToString().Replace(',', '.')},{item.RAMusagePercTotal.ToString().Replace(',', '.')}," +
                    $"{item.CPUusageDTs.ToString().Replace(',', '.')},{item.CPUusageServices.ToString().Replace(',', '.')}," +
                    $"{item.CPUusageUnity.ToString().Replace(',', '.')},{item.CPUusageTotal.ToString().Replace(',', '.')}");
                }
                writer.Close();
            }
        }
    }
}
