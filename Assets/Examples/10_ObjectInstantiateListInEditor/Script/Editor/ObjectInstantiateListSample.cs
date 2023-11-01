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

        // ProfilerWindow上に読み込んだデータの最初と最後のフレームのIndexを取得します
        int firstIndex = ProfilerDriver.firstFrameIndex;
        int lastIndex = ProfilerDriver.lastFrameIndex;

        // 全てのフレームを処理します
        for (int frameIdx = firstIndex; frameIdx < lastIndex; ++frameIdx)
        {
            // 指定フレームの0番目のThread(Main Thread)を取得します
            using (var frameData = ProfilerDriver.GetRawFrameDataView(frameIdx, 0))
            {
                // Object.RegisterのMakerIdを取得します
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
                // MainThread上にあるSampleからデータを取ります
                for (int sampleIdx = 0; sampleIdx < frameData.sampleCount; ++sampleIdx)
                {
                    // “Object.Register”が見つかった場合
                    if (frameData.GetSampleMarkerId(sampleIdx) == objRegistmaker)
                    {
                        // 生成したObjectのInstanceIdを取得します
                        var instanceId = frameData.GetSampleMetadataAsInt(sampleIdx, 0);
                        // オブジェクトに関する情報を取得します
                        if (frameData.GetUnityObjectInfo(instanceId, out var objectInfo))
                        {
                            // オブジェクトのTypeに関する情報を取得します
                            if (frameData.GetUnityObjectNativeTypeInfo(objectInfo.nativeTypeIndex,
                                out var typeInfo))
                            {
                                // 得られたTypeの名前と、オブジェクト名をログ出力します
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