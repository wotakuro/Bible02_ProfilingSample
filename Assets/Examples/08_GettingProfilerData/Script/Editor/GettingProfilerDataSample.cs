using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine.UIElements;

// Profilerのすべてのフレームからカウンターの情報を取得するサンプルです
public class GettingProfilerDataSample : EditorWindow
{
    // サンプル用のEditorWindowを作ります
    [MenuItem("Example/08_GettingProfilerData")]
    public static void Create()
    {
        EditorWindow.GetWindow<GettingProfilerDataSample>();
    }
    // 知りたい対象のカウンターを入力するためのTextField
    private TextField textField;
    // 結果表示用のScrollView
    private ScrollView scrollView;

    // EditorWindow有効化時にUIのセットアップを行います
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

    // 検索ボタンを押されたときの処理
    void OnSearch()
    {
        // TextFieldにある文字列のカウンター情報を一覧で取得してきます
        var list = GetInfoList(this.textField.value);

        // 取得してきた結果をUIに反映させます
        this.scrollView.Clear();
        foreach (var str in list)
        {
            this.scrollView.Add( new Label(str) );
        }
    }

    // 引数 counterNameに指定されたカウンターをProfilerの全フレームを検索して結果を文字列のListで返します
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
