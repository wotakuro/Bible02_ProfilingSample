using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

// Recorderによる処理時間と回数の取得
public class ProfilerRecorderSample : MonoBehaviour
{
    // 計測するためのRecorderオブジェクト
    private Recorder recorder;


    // 表示用のUI Text
    [SerializeField]
    private Text textField;

    // 毎フレーム更新するかのフラグ
    [Header("毎フレーム更新するなら updateEveryFrameにチェック¥nupdateEveryFrameにチェックがないなら１秒毎の更新")]
    [SerializeField]
    private bool updateEveryFrame;

    // 毎フレーム更新でない場合、間隔を覚えておきます
    private float updateTimer = 0.0f;

    // 開始時の処理
    void Awake()
    {
        // Animator.ProcessGraphという名前を監視します
        recorder = Recorder.Get("Animator.ProcessGraph");
        // 全スレッドにある情報を対象とします
        recorder.CollectFromAllThreads();
        // メインスレッドのみの場合はこちら
        //recorder.FilterToCurrentThread();
    }

    // Update処理
    void Update()
    {
        if (textField && recorder != null && shouldUpdate() )
        {
            // Textを更新します
            // sampleBlockCountには、呼び出された回数が、
            // elapsedNanosecondsでは、累計処理の時間（ナノ秒）があります
            textField.text = "Animator.ProcessGraph " + recorder.sampleBlockCount + "回呼び出し¥n"
                + recorder.elapsedNanoseconds + "ナノ秒かかりました";
        }
    }
    // 毎フレーム更新すると重いので、間隔を調整して更新すべきかを返します
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
