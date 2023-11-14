using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using UnityEngine.Profiling;

public class EmitFrameMetadataSample : MonoBehaviour
{// �����^�C����Ŏ��s�����R�[�h�ł�
 // FrameDataView.GetFrameMetadata�Ƌ��ʂ��Ďg���܂�
    struct TextureInfo
    {
        public int format;
        public int w;
        public int h;
    }
    // GUID�AEmitFrameMetaData��GetFrameMetaData�ň�v������K�v������܂�
    static readonly Guid MetadataId = new Guid("7E1DEA84-51F1-477A-82B5-B5C57AC1EBF7");
    // FrameMetaData��TagID�Ńf�[�^����ʂł��܂��B
    // Texture�̏��𖄂ߍ���Tag
    static readonly int TextureInfoTag = 0;
    // Texture�̖{�̂𖄂ߍ���Tag
    static readonly int TextureBodyTag = 1;



    public void LateUpdate()
    {
        StartCoroutine(RecordFrame());
    }
    // Update is called once per frame

    IEnumerator RecordFrame()
    {
        yield return new WaitForEndOfFrame();

        var texture2D = ScreenCapture.CaptureScreenshotAsTexture(1);
        if (!texture2D)
        {
            yield break;
        }
#if UNITY_EDITOR
        // Editor���s�Ŋ������ɃG���[���o��̂�h������
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            yield break;
        }
#endif
        // Texture2D�̏���Tag 0�Ԃɖ��ߍ��݂܂�
        TextureInfo textureInfo = new TextureInfo()
        {
            format = (int)texture2D.format,
            w = texture2D.width,
            h = texture2D.height
        };
        Profiler.EmitFrameMetaData(MetadataId, TextureInfoTag, new[] { textureInfo });

        // Texture�{�̂̃f�[�^���擾���ATag�P�ԂɃf�[�^�𖄂ߍ��݂܂�
        NativeArray<byte> textureData = texture2D.GetRawTextureData<byte>();
        Profiler.EmitFrameMetaData(MetadataId, TextureBodyTag, textureData);
        textureData.Dispose();
        UnityEngine.Object.Destroy(texture2D);
    }
}
