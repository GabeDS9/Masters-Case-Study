using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Test
{    public List<float> ReturnRAMUsages()
    {
        List<float> results = new List<float>();
        Process[] services = Process.GetProcessesByName("DT_Services");
        var temp = (float)services[0].WorkingSet64 / (1024 * 1024);
        results.Add(temp);
        services = Process.GetProcessesByName("Campus_DT");
        temp = (float)services[0].WorkingSet64 / (1024 * 1024);
        results.Add(temp);
        services = Process.GetProcessesByName("Unity");
        temp = (float)services[0].WorkingSet64 / (1024 * 1024);
        results.Add(temp);
        return results;
    }
}
