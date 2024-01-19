using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.Profiling;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static UnityEditor.Profiling.FrameDataView;
using System.Text;

public class GettingProfilerDataSample : EditorWindow
{

    [MenuItem("Example/08_GettingProfilerData")]
    public static void Create()
    {
        EditorWindow.GetWindow<GettingProfilerDataSample>();
    }

    private TextField textField;
    private ScrollView scrollView;
    private void OnEnable()
    {
        var button = new Button(OnSearch);
        button.text = "検索する";
        this.textField = new TextField();
        this.textField.value = "SetPass Calls Count";
        this.scrollView = new ScrollView();

        this.rootVisualElement.Add(new Label("Counterの名前を指定してください(完全一致)"));
        this.rootVisualElement.Add(textField);
        this.rootVisualElement.Add(button);
        this.rootVisualElement.Add(new Label("検索結果"));
        this.rootVisualElement.Add(scrollView);
    }

    void OnSearch()
    {
        var list = GetInfoList(this.textField.value);
        this.scrollView.Clear();
        foreach (var str in list)
        {
            this.scrollView.Add( new Label(str) );
        }
    }
    List<string> GetInfoList(string counterName) {
        List<string> list = new List<string>();
        // ProfilerWindow上に読み込んだデータの最初と最後のフレームのIndexを取得します
        int firstIndex = ProfilerDriver.firstFrameIndex;
        int lastIndex = ProfilerDriver.lastFrameIndex;

        // Profiler上の最初のフレームから最後のフレームまで処理します
        for (int frameIdx = firstIndex; frameIdx < lastIndex; ++frameIdx)
        {
            // SetPass Callsの情報をログに書き出します
            // GetFormattedCounterValueにはカテゴリを入れるのですが、
            // わからない場合nullを入れることが出来ます
            list.Add("frame:" + frameIdx + "  counter:" +
                ProfilerDriver.GetFormattedCounterValue(frameIdx, null, counterName));
        }
        return list;
    }
}
