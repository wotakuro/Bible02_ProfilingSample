using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.Profiling;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Text;

public class GettingRawFrameDataSample : EditorWindow
{
    [MenuItem("Example/09_GettingRawFrameDataInEditor")]

    public static void Create()
    {
        EditorWindow.GetWindow<GettingRawFrameDataSample>();
    }
    private TextField textField;
    private ScrollView scrollView;


    private void OnEnable()
    {
        var button = new Button(OnSearch);
        button.text = "検索する";
        this.textField = new TextField();
        this.textField.value = "Animator.ProcessGraph";
        this.scrollView = new ScrollView();

        this.rootVisualElement.Add(new Label("Profilerからリストアップしたい名前を指定してください(完全一致)"));
        this.rootVisualElement.Add(textField);
        this.rootVisualElement.Add(button);
        this.rootVisualElement.Add(new Label("検索結果"));
        this.rootVisualElement.Add(scrollView);
    }

    void OnSearch()
    {
        var list = SearchFromProfiler(this.textField.value);
        this.scrollView.Clear();
        foreach (var str in list)
        {
            this.scrollView.Add(new Label(str));
        }

    }

    // Update is called once per frame
    public List<string> SearchFromProfiler(string makerName)
    {
        var list = new List<string>();

        int firstIndex = ProfilerDriver.firstFrameIndex;
        int lastIndex = ProfilerDriver.lastFrameIndex;
        int makerId = FrameDataView.invalidMarkerId;
        FrameDataView.MarkerMetadataInfo[] metadataInfos = null;
        var stringBuilder = new StringBuilder();

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
                    if (makerId == FrameDataView.invalidMarkerId)
                    {
                        makerId = frameData.GetMarkerId(makerName);
                        if (makerId == FrameDataView.invalidMarkerId)
                        {
                            break;
                        }
                        metadataInfos = frameData.GetMarkerMetadataInfo(makerId);
                    }

                    for (int sampleIdx = 0; sampleIdx < frameData.sampleCount; ++sampleIdx)
                    {

                        if (makerId == frameData.GetSampleMarkerId(sampleIdx))
                        {
                            stringBuilder.Clear();
                            stringBuilder.Append("frame:").Append(frameIdx).Append(" Thread:").
                                Append(frameData.threadName).Append("  ").
                                Append(frameData.GetSampleTimeMs(sampleIdx)).Append("ms ");


                            if (metadataInfos != null)
                            {
                                for (int i = 0; i < metadataInfos.Length; ++i)
                                {
                                    stringBuilder.Append("\n  metadata-").Append(i).Append(" ");
                                    stringBuilder.Append(metadataInfos[i].name).Append(":");
                                    stringBuilder.Append(frameData.GetSampleMetadataAsString(sampleIdx, i));
                                }
                            }
                            list.Add(stringBuilder.ToString());
                        }
                    }
                }
            }
        }
        return list;
    }

}
