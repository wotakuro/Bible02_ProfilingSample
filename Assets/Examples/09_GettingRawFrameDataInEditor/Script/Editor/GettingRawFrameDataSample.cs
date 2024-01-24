using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor.Profiling;
using UnityEditor;
using UnityEngine.UIElements;
using System.Text;

// Profilerの全てのフレームから特定の処理を一覧で取得します
public class GettingRawFrameDataSample : EditorWindow
{
    // サンプル用のEditorWindowを作ります
    [MenuItem("Example/09_GettingRawFrameDataInEditor")]

    public static void Create()
    {
        EditorWindow.GetWindow<GettingRawFrameDataSample>();
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
        this.textField.value = "Animator.ProcessGraph";
        this.scrollView = new ScrollView();

        this.rootVisualElement.Add(new Label("ProfilerからリストアップしたいProfilerMakerの名前を指定してください(完全一致)"));
        this.rootVisualElement.Add(textField);
        this.rootVisualElement.Add(button);
        this.rootVisualElement.Add(new Label("検索結果"));
        this.rootVisualElement.Add(scrollView);
    }

    // 検索ボタンを押されたときの処理
    void OnSearch()
    {
        var list = SearchFromProfiler(this.textField.value);

        // 取得してきた結果をUIに反映させます
        this.scrollView.Clear();
        foreach (var str in list)
        {
            this.scrollView.Add(new Label(str));
        }

    }

    // makerNameに指定されたMakerの情報を一覧にしてListで返します
    public List<string> SearchFromProfiler(string makerName)
    {
        var list = new List<string>();

        // ProfilerWindow上に読み込んだデータの最初と最後のフレームのIndexを取得します
        int firstIndex = ProfilerDriver.firstFrameIndex;
        int lastIndex = ProfilerDriver.lastFrameIndex;

        // ProfilerMakerの文字列を毎回比較すると重いので、IDで検索するため用意します
        int makerId = FrameDataView.invalidMarkerId;

        FrameDataView.MarkerMetadataInfo[] metadataInfos = null;
        var stringBuilder = new StringBuilder();


        // Profiler上の最初のフレームから最後のフレームまで処理します
        for (int frameIdx = firstIndex; frameIdx < lastIndex; ++frameIdx)
        {
            // 色々なThreadの情報を取得します
            for (int threadIdx = 0; ; threadIdx++)
            {
                // ProfilerDriver経由でRawFrameDataViewオブジェクトを取得します
                using (RawFrameDataView frameData = ProfilerDriver.GetRawFrameDataView(frameIdx, threadIdx))
                {
                    // 該当データなしのため終了します
                    if (!frameData.valid)
                    {
                        break;
                    }

                    // 引数「makerName」の名前を持つ ProfilerMakerのIDを取得します
                    if (makerId == FrameDataView.invalidMarkerId)
                    {
                        makerId = frameData.GetMarkerId(makerName);
                        if (makerId == FrameDataView.invalidMarkerId)
                        {
                            break;
                        }
                        // Metaデータ情報も取得します
                        metadataInfos = frameData.GetMarkerMetadataInfo(makerId);
                    }

                    // 見つかったデータにある全てのSampleからデータを取得します
                    for (int sampleIdx = 0; sampleIdx < frameData.sampleCount; ++sampleIdx)
                    {
                        // 該当するMakerIDが一致したので、探している対象のデータです
                        if (makerId == frameData.GetSampleMarkerId(sampleIdx))
                        {
                            // 必要な情報を文字列化します
                            // frameData.GetSampleTimeMs(sampleIdx)で処理時間を取得します
                            stringBuilder.Clear();
                            stringBuilder.Append("frame:").Append(frameIdx).Append(" Thread:").
                                Append(frameData.threadName).Append("  ").
                                Append(frameData.GetSampleTimeMs(sampleIdx)).Append("ms ");

                            // MetadataがあるならMetadataの情報も文字列に追加します
                            if (metadataInfos != null)
                            {
                                for (int i = 0; i < metadataInfos.Length; ++i)
                                {
                                    stringBuilder.Append("\n  metadata-").Append(i).Append(" ");
                                    stringBuilder.Append(metadataInfos[i].name).Append(":");
                                    stringBuilder.Append(frameData.GetSampleMetadataAsString(sampleIdx, i));
                                }
                            }
                            // 上記で得た情報を文字列のListに突っ込みます
                            list.Add(stringBuilder.ToString());
                        }
                    }
                }
            }
        }
        return list;
    }

}
