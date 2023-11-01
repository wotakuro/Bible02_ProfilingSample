using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditorInternal;
using UnityEditor.Profiling;
using UnityEditor;
using UnityEditor.UIElements;

public class ObjectInstantiateListSample : EditorWindow
{
    [MenuItem("Example/10_ObjectInstantiateList")]

    public static void Create()
    {
        EditorWindow.GetWindow<ObjectInstantiateListSample>();
    }
    private void OnEnable()
    {
        var registers = GetObjectRegisterList(false);
        var unregisters = GetObjectRegisterList(true);

        var visualElement = new VisualElement();
        visualElement.style.flexDirection = FlexDirection.Row;
        visualElement.Add(CreateUIFromList("Register�ꗗ", registers));
        visualElement.Add(CreateUIFromList("Unregister�ꗗ", unregisters));

        this.rootVisualElement.Add(visualElement);
    }
    private VisualElement CreateUIFromList(string title ,List<string> list)
    {
        VisualElement visualElement = new VisualElement();
        visualElement.Add(new Label(title));
        var scrollView = new ScrollView();
        foreach (var str in list)
        {
            scrollView.Add(new Label(str));
        }
        scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        scrollView.horizontalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        visualElement.Add(scrollView);
        visualElement.style.borderLeftWidth = 2;
        visualElement.style.borderRightWidth = 2;
        return visualElement;
    }

    private List<string> GetObjectRegisterList(bool isUnregister)
    {
        var list = new List<string>();
        int objRegistmaker = FrameDataView.invalidMarkerId;

        // ProfilerWindow��ɓǂݍ��񂾃f�[�^�̍ŏ��ƍŌ�̃t���[����Index���擾���܂�
        int firstIndex = ProfilerDriver.firstFrameIndex;
        int lastIndex = ProfilerDriver.lastFrameIndex;

        // �S�Ẵt���[�����������܂�
        for (int frameIdx = firstIndex; frameIdx < lastIndex; ++frameIdx)
        {
            // �w��t���[����0�Ԗڂ�Thread(Main Thread)���擾���܂�
            using (var frameData = ProfilerDriver.GetRawFrameDataView(frameIdx, 0))
            {
                // Object.Register��MakerId���擾���܂�
                if (objRegistmaker == FrameDataView.invalidMarkerId)
                {
                    if (!isUnregister)
                    {
                        objRegistmaker = frameData.GetMarkerId("Object.Register");
                    }
                    else
                    {
                        objRegistmaker = frameData.GetMarkerId("Object.Unregister");
                    }
                }
                // MainThread��ɂ���Sample����f�[�^�����܂�
                for (int sampleIdx = 0; sampleIdx < frameData.sampleCount; ++sampleIdx)
                {
                    // �gObject.Register�h�����������ꍇ
                    if (frameData.GetSampleMarkerId(sampleIdx) == objRegistmaker)
                    {
                        // ��������Object��InstanceId���擾���܂�
                        var instanceId = frameData.GetSampleMetadataAsInt(sampleIdx, 0);
                        // �I�u�W�F�N�g�Ɋւ�������擾���܂�
                        if (frameData.GetUnityObjectInfo(instanceId, out var objectInfo))
                        {
                            // �I�u�W�F�N�g��Type�Ɋւ�������擾���܂�
                            if (frameData.GetUnityObjectNativeTypeInfo(objectInfo.nativeTypeIndex,
                                out var typeInfo))
                            {
                                // ����ꂽType�̖��O�ƁA�I�u�W�F�N�g�������O�o�͂��܂�
                                list.Add("frame:" + frameIdx + " instanceId:" + instanceId + 
                                    " name:" + objectInfo.name + " type:" + typeInfo.name );
                            }
                        }
                    }
                }
            }
        }
        return list;

    }
}