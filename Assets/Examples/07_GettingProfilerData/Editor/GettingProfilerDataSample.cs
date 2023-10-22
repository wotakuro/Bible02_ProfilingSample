using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.Profiling;
using UnityEditor;

public class GettingProfilerDataSample
{
    [MenuItem("Tools/Test")]
    // Update is called once per frame
    public static void Exec()
    {
        int firstIndex = ProfilerDriver.firstFrameIndex;
        int lastIndex = ProfilerDriver.lastFrameIndex;

        int makerId = FrameDataView.invalidMarkerId;

        for (int frameIdx = firstIndex; frameIdx < lastIndex; ++frameIdx)
        {
            for (int threadIdx = 0; ; threadIdx++)
            {
                using (RawFrameDataView frameData = ProfilerDriver.GetRawFrameDataView(frameIdx, threadIdx))
                {
                    if (!frameData.valid)
                    {
                        break;
                    }
                    makerId = frameData.GetMarkerId("Animator.ProcessGraph");
                    for(int sampleIdx = 0; sampleIdx < frameData.sampleCount; ++sampleIdx)
                    {
                        
                        if(makerId == frameData.GetSampleMarkerId(sampleIdx))
                        {
                            Debug.Log("Animator.ProcessGraph " + frameData.threadName+"::" + frameData.GetSampleTimeMs(sampleIdx));
                        }
                    }
                }
            }
        }
    }
}
