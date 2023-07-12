using UnityEditor;
using UnityEngine;

/// <summary> This class does not have any impact on the inventory system behavior, I just want to limit my GPU utilization </summary>
public class FpsLock
{
    static FpsLock()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 70;
    }
}
