using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class GettingCounterSample : MonoBehaviour
{
    // Draw Calls Count�̒l�擾�p�I�u�W�F�N�g
    ProfilerRecorder drawCallsCount;

    // SetPass Calls Count�̒l�擾�p�I�u�W�F�N�g
    ProfilerRecorder setPassCount;
    // UsedMemory�̒l�擾�p�I�u�W�F�N�g
    ProfilerRecorder usedMemory;


    // Start is called before the first frame update
    void Awake()
    {
        drawCallsCount = ProfilerRecorder.StartNew(ProfilerCategory.Render,
                                                       "Draw Calls Count");
        setPassCount = ProfilerRecorder.StartNew(ProfilerCategory.Render,
                                                 "SetPass Calls Count");
        usedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory,
                                                 "Total Used Memory");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("drawCall:" + drawCallsCount.LastValue +
            "setPassCount:" + setPassCount.LastValue +
            "usedMemory:" + usedMemory.LastValue);
    }
}
