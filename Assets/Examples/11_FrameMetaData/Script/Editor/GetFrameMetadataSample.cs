
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

    // Editor��̃R�[�h�ł�
    // GUID�AEmitFrameMetaData��GetFrameMetaData�ň�v������K�v������܂�
    Guid MetadataId = new Guid("7E1DEA84-51F1-477A-82B5-B5C57AC1EBF7");
    // FrameMetaData��TagID�Ńf�[�^����ʂł��܂��B
    // Texture�̏��𖄂ߍ��܂ꂽTag
    static readonly int TextureInfoTag = 0;
    // Texture�̖{�̂𖄂ߍ��܂ꂽTag
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
        // ProfilerWindow�Ō��ݑI�𒆂̃t���[����Index
        int frameIdx = (int)ProfilerWindow.GetWindow<ProfilerWindow>().selectedFrameIndex;
        if (cacheFrameIdx != frameIdx)
        {
            // ������Texture��j�����܂�
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
            EditorGUILayout.LabelField("Screenshot�̃f�[�^�����ߍ��܂�Ă��܂���");
            EditorGUILayout.LabelField("11_FrameMetaData�̃V�[���Ŏ��s����Profiler��Screenshot�𖄂ߍ���ł�������");
        }

    }
    private Texture2D GetScreenshotTexture(int frameIdx)
    {

        // ProfilerWindow�̌��ݑI�𒆂̃t���[���̏��𓾂܂�
        using (var frameData = ProfilerDriver.GetHierarchyFrameDataView(frameIdx, 0,
            HierarchyFrameDataView.ViewModes.Default,
            HierarchyFrameDataView.columnDontSort, false))
        {
            // �t���[���ɖ��ߍ��܂ꂽ MetadataId �̃^�O�O�Ԃ�TextureInfo�^�Ƃ��Ď擾���܂�
            NativeArray<TextureInfo> textureInfos =
                frameData.GetFrameMetaData<TextureInfo>(MetadataId, TextureInfoTag);

            if (textureInfos == null || textureInfos.Length == 0)
            {
                return null;
            }

            // Texture�̏�񂩂�Texture���쐬���܂�
            TextureInfo textureInfo = textureInfos[0];
            Texture2D texture2D = new Texture2D(textureInfo.w, textureInfo.h,
                (TextureFormat)textureInfo.format, false);

            // �t���[���ɖ��ߍ��܂ꂽ MetadataId �̃^�O1�Ԃ�Byte�z��Ƃ��Ď擾���܂�
            NativeArray<byte> textureData =
                frameData.GetFrameMetaData<byte>(MetadataId, TextureBodyTag);
            // Texture�̖{�̂�����Texture�̏������[�h���܂�
            texture2D.LoadRawTextureData(textureData);
            texture2D.Apply();
            return texture2D;
        }

    }
}