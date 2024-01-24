using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using UnityEngine.Profiling;

//EmitFrameMetaDataによる埋め込みのサンプルです
public class EmitFrameMetadataSample : MonoBehaviour
{// ランタイム上で実行されるコードです
 // FrameDataView.GetFrameMetadataと共通して使います
    struct TextureInfo
    {
        public int format;
        public int w;
        public int h;
    }
    // GUID、EmitFrameMetaDataとGetFrameMetaDataで一致させる必要があります
    static readonly Guid MetadataId = new Guid("7E1DEA84-51F1-477A-82B5-B5C57AC1EBF7");
    // FrameMetaDataはTagIDでデータを区別できます。
    // Textureの情報を埋め込むTag
    static readonly int TextureInfoTag = 0;
    // Textureの本体を埋め込むTag
    static readonly int TextureBodyTag = 1;


    // LateUpdate処理です
    public void LateUpdate()
    {
        StartCoroutine(RecordFrame());
    }

    // コルーチンの処理です
    IEnumerator RecordFrame()
    {
        yield return new WaitForEndOfFrame();
        // Texture2DをキャプチャしてTexture2Dにします
        var texture2D = ScreenCapture.CaptureScreenshotAsTexture(1);
        if (!texture2D)
        {
            yield break;
        }
#if UNITY_EDITOR
        // Editor実行で完了時にエラーが出るのを防ぐため
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            yield break;
        }
#endif
        // Texture2Dの情報をTag 0番に埋め込みます
        TextureInfo textureInfo = new TextureInfo()
        {
            format = (int)texture2D.format,
            w = texture2D.width,
            h = texture2D.height
        };
        Profiler.EmitFrameMetaData(MetadataId, TextureInfoTag, new[] { textureInfo });

        // Texture本体のデータを取得し、Tag１番にデータを埋め込みます
        NativeArray<byte> textureData = texture2D.GetRawTextureData<byte>();
        Profiler.EmitFrameMetaData(MetadataId, TextureBodyTag, textureData);
        // Screenshotデータを破棄する処理をします
        textureData.Dispose();
        UnityEngine.Object.Destroy(texture2D);
    }
}
