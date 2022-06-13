using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class MemoryProfiler : MonoBehaviour
{
    ProfilerRecorder _totalMemoryRecorder;
    float memoryValue = 0;
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
            memoryValue = (float)_totalMemoryRecorder.LastValueAsDouble;
        }
    }
    public float ReturnMemoryReserved()
    {
        return memoryValue / (1024 * 1024);
    }
}
