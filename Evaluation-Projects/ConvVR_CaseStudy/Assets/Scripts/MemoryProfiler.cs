using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

public class MemoryProfiler : MonoBehaviour
{
    ProfilerRecorder _totalMemoryRecorder;
    float memoryValue = 0;
    void OnEnable()
    {
        _totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
    }
    void OnDisable()
    {
        _totalMemoryRecorder.Dispose();
    }
    void Update()
    {
        if (_totalMemoryRecorder.Valid)
        {
            memoryValue = (float)_totalMemoryRecorder.LastValueAsDouble;
        }
    }
    public float ReturnMemoryReserved()
    {
        return memoryValue / (1024*1024);
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
