using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

public class TestService
{
    public async Task<List<float>> ReturnUsagesAsync()
    {
        List<float> results = new List<float>();
        string servProc = "DT_Services";
        string dtProc = "Campus_DT";
        string vrProc = "Unity";
        Process[] services = Process.GetProcessesByName(servProc);
        var temp = (float)services[0].WorkingSet64 / (1024*1024);
        results.Add(temp);
        results.Add(await ReturnCurrentProcessCPU());
        services = Process.GetProcessesByName(dtProc);
        temp = (float)services[0].WorkingSet64 / (1024 * 1024);
        results.Add(temp);
        results.Add(await ReturnCPUUsages(dtProc));
        services = Process.GetProcessesByName(vrProc);
        temp = (float)services[0].WorkingSet64 / (1024 * 1024);
        results.Add(temp);
        results.Add(await ReturnCPUUsages(vrProc));
        return results;
    }
    public async Task<float> ReturnCPUUsages(string processName)
    {
        var startTime = DateTime.UtcNow;
        var startCPUUsage = Process.GetProcessesByName(processName)[0].TotalProcessorTime;
        await Task.Delay(500);
        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetProcessesByName(processName)[0].TotalProcessorTime;
        var cpuUsedMs = (float)(endCpuUsage - startCPUUsage).TotalMilliseconds;
        var totalMsPassed = (float)(endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        var perc = cpuUsageTotal * 100;
        return perc;
    }
    public async Task<float> ReturnCurrentProcessCPU()
    {
        var startTime = DateTime.UtcNow;
        var startCPUUsage = Process.GetCurrentProcess().TotalProcessorTime;
        await Task.Delay(500);
        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        var cpuUsedMs = (float)(endCpuUsage - startCPUUsage).TotalMilliseconds;
        var totalMsPassed = (float)(endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        return cpuUsageTotal * 100;
    }
}
