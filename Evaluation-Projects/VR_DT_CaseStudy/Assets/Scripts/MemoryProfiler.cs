using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class MemoryProfiler : MonoBehaviour
{
    ProfilerRecorder _totalMemoryRecorder;
    double maxMemory = 0;
    void OnEnable()
    {
        _totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
    }
    void OnDisable()
    {
        _totalMemoryRecorder.Dispose();
    }
    void Update()
    {
        if (_totalMemoryRecorder.Valid)
        {
            if(_totalMemoryRecorder.LastValue > maxMemory)
            {
                maxMemory = _totalMemoryRecorder.LastValueAsDouble;
            }
        }
    }
    public double ReturnMemoryReserved()
    {
        var temp = maxMemory / (1024.0 * 1024.0);
        maxMemory = 0;
        return temp;
    }
}
