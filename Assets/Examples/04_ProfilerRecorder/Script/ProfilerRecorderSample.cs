using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class ProfilerRecorderSample : MonoBehaviour
{
    private Recorder recorder;
    [SerializeField]
    private Text textField;

    [Header("毎フレーム更新するなら updateEveryFrameにチェック¥nupdateEveryFrameにチェックがないなら１秒毎の更新")]
    [SerializeField]
    private bool updateEveryFrame;

    private float updateTimer = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        recorder = Recorder.Get("Animator.ProcessGraph");
        recorder.CollectFromAllThreads();
        // メインスレッドのみの場合
        //recorder.FilterToCurrentThread();
    }

    // Update is called once per frame
    void Update()
    {
        if (textField && recorder != null && shouldUpdate() )
        {
            textField.text = "Animator.ProcessGraph " + recorder.sampleBlockCount + "回呼び出し¥n"
                + recorder.elapsedNanoseconds + "ナノ秒かかりました";
        }
    }
    private bool shouldUpdate()
    {
        if (updateEveryFrame)
        {
            return true;
        }
        updateTimer -= Time.deltaTime;
        if(updateTimer < 0.0f)
        {
            updateTimer = 1.0f;
            return true;
        }
        return false;
    }
}
