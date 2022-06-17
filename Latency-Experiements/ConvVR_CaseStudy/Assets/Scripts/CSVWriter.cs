using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Models;

public class CSVWriter : MonoBehaviour
{
    string Conv_Single_DTI_Single_Data_File = "";
    string Conv_Multiple_DTI_Single_Data = "";
    string Conv_Single_DTI_Multiple_Data = "";
    string Conv_Multiple_DTI_Multiple_Data = "";
    string Conv_Aggregate_Single_Data = "";
    string Conv_Aggregate_Multiple_Data = "";
    string Local_Conv_Single_DTI_Single_Data_File = "";
    string Local_Conv_Multiple_DTI_Single_Data = "";
    string Local_Conv_Single_DTI_Multiple_Data = "";
    string Local_Conv_Multiple_DTI_Multiple_Data = "";
    string Local_Conv_Aggregate_Single_Data = "";
    string Local_Conv_Aggregate_Multiple_Data = "";
    List<string> fileNames = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        /*Conv_Single_DTI_Single_Data_File = Application.dataPath + "/Conv_Single_DTI_Single_Data_File.csv";
        fileNames.Add(Conv_Single_DTI_Single_Data_File);
        Conv_Multiple_DTI_Single_Data = Application.dataPath + "/Conv_Multiple_DTI_Single_Data.csv";
        fileNames.Add(Conv_Multiple_DTI_Single_Data);
        Conv_Single_DTI_Multiple_Data = Application.dataPath + "/Conv_Single_DTI_Multiple_Data.csv";
        fileNames.Add(Conv_Single_DTI_Multiple_Data);
        Conv_Multiple_DTI_Multiple_Data = Application.dataPath + "/Conv_Multiple_DTI_Multiple_Data.csv";
        fileNames.Add(Conv_Multiple_DTI_Multiple_Data);
        Conv_Aggregate_Single_Data = Application.dataPath + "/Conv_Aggregate_Single_Data.csv";
        fileNames.Add(Conv_Aggregate_Single_Data);
        Conv_Aggregate_Multiple_Data = Application.dataPath + "/Conv_Aggregate_Multiple_Data.csv";
        fileNames.Add(Conv_Aggregate_Multiple_Data);*/
        Local_Conv_Single_DTI_Single_Data_File = Application.dataPath + "/Local_Conv_Single_DTI_Single_Data_File.csv";
        fileNames.Add(Local_Conv_Single_DTI_Single_Data_File);
        Local_Conv_Multiple_DTI_Single_Data = Application.dataPath + "/Local_Conv_Multiple_DTI_Single_Data.csv";
        fileNames.Add(Local_Conv_Multiple_DTI_Single_Data);
        Local_Conv_Single_DTI_Multiple_Data = Application.dataPath + "/Local_Conv_Single_DTI_Multiple_Data.csv";
        fileNames.Add(Local_Conv_Single_DTI_Multiple_Data);
        Local_Conv_Multiple_DTI_Multiple_Data = Application.dataPath + "/Local_Conv_Multiple_DTI_Multiple_Data.csv";
        fileNames.Add(Local_Conv_Multiple_DTI_Multiple_Data);
        Local_Conv_Aggregate_Single_Data = Application.dataPath + "/Local_Conv_Aggregate_Single_Data.csv";
        fileNames.Add(Local_Conv_Aggregate_Single_Data);
        Local_Conv_Aggregate_Multiple_Data = Application.dataPath + "/Local_Conv_Aggregate_Multiple_Data.csv";
        fileNames.Add(Local_Conv_Aggregate_Multiple_Data);
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
                    $"{item.RAMusageMBTotal.ToString().Replace(',', '.')},{item.RAMusagePercTotal.ToString().Replace(',', '.')}," +
                    $"{item.CPUusageTotal.ToString().Replace(',', '.')}");
                }
                writer.Close();
            }
        }
    }
}
