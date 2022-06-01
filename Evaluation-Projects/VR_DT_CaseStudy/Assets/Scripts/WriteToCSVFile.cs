using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

public class WriteToCSVFile
{
    static string filename = "LatencyEvaluation";
    string filepath = filename + ".csv";
    public void addRecords(List<EvaluationTestModel> testInfoList)
    {
        try
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
        }
    }
}
