using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class GettingCounterSample : MonoBehaviour
{

    [SerializeField]
    private Text textField;

    // Draw Calls Countの値取得用オブジェクト
    private ProfilerRecorder drawCallsCount;
    // SetPass Calls Countの値取得用オブジェクト
    private ProfilerRecorder setPassCount;
    // UsedMemoryの値取得用オブジェクト
    private ProfilerRecorder usedMemory;


    // Start is called before the first frame update
    void Awake()
    {
        // Editor上での実行で描画スキップ？されて０になるので対策
        Application.targetFrameRate = 30;

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
        if (textField)
        {
            textField.text = "drawCall:" + drawCallsCount.LastValue + "\n" +
                "setPassCount:" + setPassCount.LastValue + "\n" +
                "usedMemory:" + usedMemory.LastValue;
        }
    }
}
