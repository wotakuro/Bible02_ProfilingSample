using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.UI;
using System.Text;
using Unity.Profiling.LowLevel.Unsafe;

// リリースビルドでも取得可能なRecorder一覧を取得
public class ReleaseBuildProfileSample : MonoBehaviour
{
    // 表示するテキスト
    [SerializeField]
    public Text info;
    // 表示のScrollView
    [SerializeField]
    public ScrollRect scrollRect;
    // 実際に取れるものRecorderのHandle一覧
    List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>();

    // Recorderの名前一覧を保持します
    private List<string> recordNames = new List<string>();

    // 実際のRecorder一覧です
    private List<ProfilerRecorder> recorders = new List<ProfilerRecorder>();

    // 文字列表示用のStringBuilder
    private StringBuilder stringBuilder = new StringBuilder();

    // Start処理
    void Start()
    {
        // ProfilerRecordのHandle一覧を取得します
        ProfilerRecorderHandle.GetAvailable(this.handles);
        foreach (var handle in handles)
        {
            if (handle.Valid)
            {
                // Handleから名前を取得します
                var statDesc = ProfilerRecorderHandle.GetDescription(handle);
                recordNames.Add(statDesc.Name);
                // 名前からRecorderを取得します
                var recorder = new ProfilerRecorder(statDesc.Name);
                // Record開始します
                recorder.Start();
                recorders.Add(recorder);
            }
        }
    }

    // Update処理
    void Update()
    {
        // 30フレームに１度更新します
        if (Time.frameCount % 30 != 0) { return; }
        stringBuilder.Clear();
        stringBuilder.Append("count ").Append(recordNames.Count).Append("\n");
        // Recorder一覧を書き出します
        for (int i = 0; i < recordNames.Count; i++)
        {
            stringBuilder.Append(recordNames[i]).Append("::").
                    Append(recorders[i].LastValueAsDouble).Append("\n");
        }
        // 文字数長すぎるとエラーになるので・・・文字数減らします
        if(stringBuilder.Length > 1024 * 15)
        {
            stringBuilder.Length = 1024 * 15;
            stringBuilder.Append("...");
        }
        // 文字を書き出します
        if (info)
        {
            info.text = stringBuilder.ToString();
        }
        // Scrollのサイズをいい感じにしときます
        if (scrollRect && info)
        {
           scrollRect.content.sizeDelta = new Vector2( info.preferredWidth , info.preferredHeight);
        }
    }
}
