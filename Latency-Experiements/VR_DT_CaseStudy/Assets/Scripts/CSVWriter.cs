using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Models;

public class CSVWriter : MonoBehaviour
{
    string DT_Single_DTI_Single_Data_File = "";
    string DT_Multiple_DTI_Single_Data = "";
    string DT_Single_DTI_Multiple_Data = "";
    string DT_Multiple_DTI_Multiple_Data = "";
    string DT_Aggregate_Single_Data = "";
    string DT_Aggregate_Multiple_Data = "";
    List<string> fileNames = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        DT_Single_DTI_Single_Data_File = Application.dataPath + "/DT_Single_DTI_Single_Data.csv";
        fileNames.Add(DT_Single_DTI_Single_Data_File);
        DT_Multiple_DTI_Single_Data = Application.dataPath + "/DT_Multiple_DTI_Single_Data.csv";
        fileNames.Add(DT_Multiple_DTI_Single_Data);
        DT_Single_DTI_Multiple_Data = Application.dataPath + "/DT_Single_DTI_Multiple_Data.csv";
        fileNames.Add(DT_Single_DTI_Multiple_Data);
        DT_Multiple_DTI_Multiple_Data = Application.dataPath + "/DT_Multiple_DTI_Multiple_Data.csv";
        fileNames.Add(DT_Multiple_DTI_Multiple_Data);
        DT_Aggregate_Single_Data = Application.dataPath + "/DT_Aggregate_Single_Data.csv";
        fileNames.Add(DT_Aggregate_Single_Data);
        DT_Aggregate_Multiple_Data = Application.dataPath + "/DT_Aggregate_Multiple_Data.csv";
        fileNames.Add(DT_Aggregate_Multiple_Data);
    }

    public void WriteCSV(List<EvaluationTestModel> testInfoList, int fileNum)
    {
        string filename = fileNames[fileNum];
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
