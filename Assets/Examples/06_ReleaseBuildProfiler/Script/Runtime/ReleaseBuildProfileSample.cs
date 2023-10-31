using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.UI;
using System.Text;
using Unity.Profiling.LowLevel.Unsafe;

public class ReleaseBuildProfileSample : MonoBehaviour
{

    public Text info;
    public ScrollRect scrollRect;

    List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>();

    private List<string> recordNames;
    private List<ProfilerRecorder> recorders;
    private StringBuilder stringBuilder = new StringBuilder();
    // Start is called before the first frame update
    void Start()
    {
        this.recordNames = new List<string>();
        this.recorders = new List<ProfilerRecorder>();

        ProfilerRecorderHandle.GetAvailable(this.handles);
        foreach (var handle in handles)
        {
            if (handle.Valid)
            {
                var statDesc = ProfilerRecorderHandle.GetDescription(handle);
                recordNames.Add(statDesc.Name);
                var recorder = new Unity.Profiling.ProfilerRecorder(statDesc.Name);
                recorder.Start();
                recorders.Add(recorder);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 30 != 0) { return; }
        stringBuilder.Clear();
        stringBuilder.Append("count ").Append(recordNames.Count).Append("\n");
        for (int i = 0; i < recordNames.Count; i++)
        {
            stringBuilder.Append(recordNames[i]).Append("::").
                    Append(recorders[i].LastValueAsDouble).Append("\n");
        }
        if(stringBuilder.Length > 1024 * 15)
        {
            stringBuilder.Length = 1024 * 15;
            stringBuilder.Append("...");
        }

        if (info)
        {
            info.text = stringBuilder.ToString();
        }
        if (scrollRect && info)
        {
           scrollRect.content.sizeDelta = new Vector2( info.preferredWidth , info.preferredHeight);
        }
    }
}
