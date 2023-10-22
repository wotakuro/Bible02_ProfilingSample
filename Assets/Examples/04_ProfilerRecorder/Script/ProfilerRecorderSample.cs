using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

public class ProfilerRecorderSample : MonoBehaviour
{
    Recorder recorder;

    // Start is called before the first frame update
    void Awake()
    {
        recorder = Recorder.Get("Animator.ProcessGraph");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("loopTime " + recorder.sampleBlockCount + "::" + recorder.elapsedNanoseconds);   
    }
}
