using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Test     
{
    MemoryProfiler memoryProfiler = new MemoryProfiler();
    public float ReturnRAMUsage()
    {
        /*var process = Process.GetProcesses();
        float temp = (float)process[0].WorkingSet64 / (1024 * 1024);*/
        var memoryUsageMB = (float)memoryProfiler.ReturnMemoryReserved();
        return memoryUsageMB;
    }
}
