using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.Profiling;
using UnityEditor;
using UnityEditor.UI;

public class ObjectInstantiateListSample : EditorWindow
{
    [MenuItem("Example/10_ObjectInstantiateList")]

    public static void Create()
    {
        EditorWindow.GetWindow<ObjectInstantiateListSample>();
    }
    private void OnEnable()
    {
        GetObjectRegisterList(false);
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
                    if (isUnregister)
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
                                list.Add("frame:" + frameIdx + "  " +
                                    typeInfo.name + "::" + objectInfo.name);
                            }
                        }
                    }
                }
            }
        }
        return list;

    }
}