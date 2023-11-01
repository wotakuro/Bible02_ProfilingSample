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
        button.text = "��������";
        this.textField = new TextField();
        this.textField.value = "SetPass Calls Count";
        this.scrollView = new ScrollView();

        this.rootVisualElement.Add(new Label("Profiler���烊�X�g�A�b�v���������O���w�肵�Ă�������(���S��v)"));
        this.rootVisualElement.Add(textField);
        this.rootVisualElement.Add(button);
        this.rootVisualElement.Add(new Label("��������"));
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
        // ProfilerWindow��ɓǂݍ��񂾃f�[�^�̍ŏ��ƍŌ�̃t���[����Index���擾���܂�
        int firstIndex = ProfilerDriver.firstFrameIndex;
        int lastIndex = ProfilerDriver.lastFrameIndex;

        // Profiler��̍ŏ��̃t���[������Ō�̃t���[���܂ŏ������܂�
        for (int frameIdx = firstIndex; frameIdx < lastIndex; ++frameIdx)
        {
            // SetPass Calls�̏������O�ɏ����o���܂�
            // GetFormattedCounterValue�ɂ̓J�e�S��������̂ł����A
            // �킩��Ȃ��ꍇnull�����邱�Ƃ��o���܂�
            list.Add("frame:" + frameIdx + "  counter:" +
                ProfilerDriver.GetFormattedCounterValue(frameIdx, null, counterName));
        }
        return list;
    }
}
