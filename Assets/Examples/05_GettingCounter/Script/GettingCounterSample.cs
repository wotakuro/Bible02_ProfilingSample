using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class GettingCounterSample : MonoBehaviour
{

    [SerializeField]
    private Text textField;

    // Draw Calls Count�̒l�擾�p�I�u�W�F�N�g
    private ProfilerRecorder drawCallsCount;
    // SetPass Calls Count�̒l�擾�p�I�u�W�F�N�g
    private ProfilerRecorder setPassCount;
    // UsedMemory�̒l�擾�p�I�u�W�F�N�g
    private ProfilerRecorder usedMemory;


    // Start is called before the first frame update
    void Awake()
    {
        // Editor��ł̎��s�ŕ`��X�L�b�v�H����ĂO�ɂȂ�̂ő΍�
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
