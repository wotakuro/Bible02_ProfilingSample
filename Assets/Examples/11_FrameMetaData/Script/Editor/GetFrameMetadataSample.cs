
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

public class GetFrameMetadataSample : EditorWindow
{
    struct TextureInfo
    {
        public int format;
        public int w;
        public int h;
    }
    [MenuItem("Example/11_FrameMetaData")]
    public static void Create()
    {
        GetFrameMetadataSample.GetWindow<GetFrameMetadataSample>();
    }

    // Editor上のコードです
    // GUID、EmitFrameMetaDataとGetFrameMetaDataで一致させる必要があります
    Guid MetadataId = new Guid("7E1DEA84-51F1-477A-82B5-B5C57AC1EBF7");
    // FrameMetaDataはTagIDでデータを区別できます。
    // Textureの情報を埋め込まれたTag
    static readonly int TextureInfoTag = 0;
    // Textureの本体を埋め込まれたTag
    static readonly int TextureBodyTag = 1;

    private void OnEnable()
    {
        ProfilerWindow.GetWindow<ProfilerWindow>().SelectedFrameIndexChanged += (frame) =>
        {
            this.Repaint();
        };
    }
    private Texture2D screenshotCache;
    private int cacheFrameIdx = -1;

    // Update is called once per frame
    void OnGUI()
    {        
        // ProfilerWindowで現在選択中のフレームのIndex
        int frameIdx = (int)ProfilerWindow.GetWindow<ProfilerWindow>().selectedFrameIndex;
        if (cacheFrameIdx != frameIdx)
        {
            // 既存のTextureを破棄します
            if (screenshotCache)
            {
                UnityEngine.Object.Destroy(screenshotCache);
            }
            screenshotCache = GetScreenshotTexture(frameIdx);
            cacheFrameIdx = frameIdx;
        }
        if (screenshotCache)
        {
            EditorGUI.DrawPreviewTexture(new Rect(10, 10, screenshotCache.width, screenshotCache.height), screenshotCache);

        }
        else
        {
            EditorGUILayout.LabelField("Screenshotのデータが埋め込まれていません");
            EditorGUILayout.LabelField("11_FrameMetaDataのシーンで実行してProfilerにScreenshotを埋め込んでください");
        }

    }
    private Texture2D GetScreenshotTexture(int frameIdx)
    {

        // ProfilerWindowの現在選択中のフレームの情報を得ます
        using (var frameData = ProfilerDriver.GetHierarchyFrameDataView(frameIdx, 0,
            HierarchyFrameDataView.ViewModes.Default,
            HierarchyFrameDataView.columnDontSort, false))
        {
            // フレームに埋め込まれた MetadataId のタグ０番をTextureInfo型として取得します
            NativeArray<TextureInfo> textureInfos =
                frameData.GetFrameMetaData<TextureInfo>(MetadataId, TextureInfoTag);

            if (textureInfos == null || textureInfos.Length == 0)
            {
                return null;
            }

            // Textureの情報からTextureを作成します
            TextureInfo textureInfo = textureInfos[0];
            Texture2D texture2D = new Texture2D(textureInfo.w, textureInfo.h,
                (TextureFormat)textureInfo.format, false);

            // フレームに埋め込まれた MetadataId のタグ1番をByte配列として取得します
            NativeArray<byte> textureData =
                frameData.GetFrameMetaData<byte>(MetadataId, TextureBodyTag);
            // Textureの本体を元にTextureの情報をロードします
            texture2D.LoadRawTextureData(textureData);
            texture2D.Apply();
            return texture2D;
        }

    }
}