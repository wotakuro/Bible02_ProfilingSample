﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using Unity.Collections.LowLevel.Unsafe;

#if DEBUG
namespace UTJ.SS2Profiler
{

    internal class ScreenShotLogic : System.IDisposable
    {

        private const int FRAME_NUM = 8;

        private struct DataInfo
        {
            public RenderTexture renderTexture;
            public int id;
            public bool isRequest;
            public int fromEnd;
            public ScreenShotToProfiler.TextureCompress compress;
        }
        private struct RequestInfo
        {
            public AsyncGPUReadbackRequest request;
            public int idx;
        }

        private Queue<RequestInfo> requests = new Queue<RequestInfo>();
        private DataInfo[] frames;
        private byte[] tagInfo;

        private Texture2D syncTexCache;
        private CustomSampler syncUpdateSampler;

        private CustomSampler rgbCompressSampler;
        private CustomSampler pngCompressSampler;
        private CustomSampler jpgCompressSampler;
        private ScreenShotToProfiler.TextureCompress compress;

        public System.Action<RenderTexture> captureBehaviour { get; set; }

        public ScreenShotLogic(int width , int height, ScreenShotToProfiler.TextureCompress comp)
        {
            RenderTextureFormat format = ScreenShotProfilerUtil.GetRenderTextureFormat(comp);
            frames = new DataInfo[FRAME_NUM];
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                frames[i].renderTexture = new RenderTexture(width, height, 0, format);
                frames[i].renderTexture.name = "ss2profiler_" + i;
                frames[i].isRequest = false;
                frames[i].fromEnd = 5;
            }
            this.tagInfo = new byte[16];
            this.WriteToTagInfoShort(width, 4);
            this.WriteToTagInfoShort(height, 6);
            this.tagInfo[12] = (byte)comp;
            this.compress = comp;
        }

        private void WriteToTagInfo(int val,int idx)
        {
            tagInfo[idx + 0] = (byte)((val >> 0 )& 0xff);
            tagInfo[idx + 1] = (byte)((val >> 8 ) & 0xff);
            tagInfo[idx + 2] = (byte)((val >> 16) & 0xff);
            tagInfo[idx + 3] = (byte)((val >> 24) & 0xff);
        }
        private void WriteToTagInfoShort(int val, int idx)
        {
            tagInfo[idx  + 0] = (byte)((val >> 0) & 0xff);
            tagInfo[idx  + 1] = (byte)((val >> 8) & 0xff);
        }

        public void Dispose()
        {
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                frames[i].renderTexture.Release();
                frames[i].isRequest = false;
            }
            frames = null;
        }

        public void UpdateAsyncRequest()
        {
            for( int i = 0; i < FRAME_NUM; ++i)
            {
                if (!frames[i].isRequest)
                {
                    frames[i].fromEnd++;
                }
            }
            while (requests.Count > 0)
            {
                var req = requests.Peek();
                int idx = req.idx;
                if (req.request.hasError)
                {
                    Debug.LogError("GPU readback error detected.");
                    //[req.idx]   
                    frames[idx].isRequest = false;
                    frames[idx].fromEnd = 0;

                    requests.Dequeue();                    
                }
                else if (req.request.done)
                {
                    var data = req.request.GetData<byte>();
                    this.EmitCaptureBodyData( frames[idx].compress,frames[idx].id, data,
                        frames[idx].renderTexture);

                    frames[idx].isRequest = false;
                    frames[idx].fromEnd = 0;

                    requests.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        public void ReadBackSyncAtIdx(int idx)
        {
            if (idx < 0 || idx >= FRAME_NUM)
            {
                return;
            }
            var rt = frames[idx].renderTexture;
            if( rt == null) { return; }
            if(syncUpdateSampler == null)
            {
                this.syncUpdateSampler = CustomSampler.Create("SyncUpdate");
            }
            TextureFormat textureFormat = ScreenShotProfilerUtil.GetTextureFormat(frames[idx].compress);
            syncUpdateSampler.Begin();
            if (syncTexCache != null &&
                (syncTexCache.width != rt.width || syncTexCache.height != rt.height || 
                textureFormat != syncTexCache.format) )
            {
                Object.Destroy(syncTexCache);
                syncTexCache = null;
            }

            Texture2D tex2d = syncTexCache;
            if (tex2d == null)
            {
                tex2d = new Texture2D(rt.width, rt.height, textureFormat, false);
            }
            RenderTexture.active = rt;
            tex2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex2d.Apply();
            var bytes =  tex2d.GetRawTextureData<byte>();
            
            this.EmitCaptureBodyData( frames[idx].compress, frames[idx].id, bytes,
                frames[idx].renderTexture);

            syncTexCache = tex2d;

            frames[idx].isRequest = false;
            frames[idx].fromEnd = 0;
            syncUpdateSampler.End();
        }

        private unsafe NativeArray<byte> ConvertToRGBData(NativeArray<byte> bytes,int pixel)
        {
            if(rgbCompressSampler == null) { rgbCompressSampler = CustomSampler.Create("RGBCompress"); }
            rgbCompressSampler.Begin();
            void* bytesHead = NativeArrayUnsafeUtility.GetUnsafePtr(bytes);


            NativeArray<byte> data = 
                NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bytesHead, pixel * 3, Allocator.None);
            rgbCompressSampler.End();
            return data;
        }


        private void EmitCaptureBodyData(ScreenShotToProfiler.TextureCompress comp,int id, 
            NativeArray<byte> bytes, RenderTexture originRt)
        {
            switch (comp) {
                case ScreenShotToProfiler.TextureCompress.None:
                    Profiler.EmitFrameMetaData(ScreenShotToProfiler.MetadataGuid, id, bytes);
                    break;
                case ScreenShotToProfiler.TextureCompress.RGB_565:
                    Profiler.EmitFrameMetaData(ScreenShotToProfiler.MetadataGuid, id, bytes);
                    break;
                case ScreenShotToProfiler.TextureCompress.PNG:
                    {
                        if (pngCompressSampler == null) { pngCompressSampler = CustomSampler.Create("pngCompress"); }
                        pngCompressSampler.Begin();
                        using (var pngData = ImageConversion.EncodeNativeArrayToPNG(bytes,
                            originRt.graphicsFormat, (uint)originRt.width, (uint)originRt.height))
                        {
                            Profiler.EmitFrameMetaData(ScreenShotToProfiler.MetadataGuid, id, pngData);
                        }
                        pngCompressSampler.End();
                    }
                    break;
                case ScreenShotToProfiler.TextureCompress.JPG_BufferRGB565:
                case ScreenShotToProfiler.TextureCompress.JPG_BufferRGBA:
                    {
                        if (jpgCompressSampler == null) { jpgCompressSampler = CustomSampler.Create("jpgCompress"); }
                        jpgCompressSampler.Begin();
                        using (var pngData = ImageConversion.EncodeNativeArrayToJPG(bytes,
                            originRt.graphicsFormat, (uint)originRt.width, (uint)originRt.height))
                        {
                            Profiler.EmitFrameMetaData(ScreenShotToProfiler.MetadataGuid, id, pngData);
                        }
                        jpgCompressSampler.End();
                    }
                    break;
            }

        }

        public void AsyncReadbackRequestAtIdx(int idx)
        {
            if ( idx < 0 || idx >= FRAME_NUM)
            {
                return;
            }
            if (!IsAvailable(idx) ) { return; }
            //            var request = AsyncGPUReadback.RequestIntoNativeArray(ref frames[idx].data, frames[idx].renderTexture, 0);
            var request = AsyncGPUReadback.Request(frames[idx].renderTexture);
            requests.Enqueue( new RequestInfo { request = request, idx = idx });

            frames[idx].isRequest = true;
            frames[idx].fromEnd = 0;
        }

        private bool IsAvailable(int idx)
        {
            return ((!frames[idx].isRequest) && frames[idx].fromEnd >=2 );
        }

        public int CaptureScreen(int id)
        { 
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                if (!IsAvailable(i))
                {
                    continue;
                }
                if(captureBehaviour == null) { continue; }
                frames[i].id = id;
                frames[i].compress = this.compress;
                WriteTagMetaData(id);
                captureBehaviour(frames[i].renderTexture);
                return i;
            }
            return -1;
        }
        private void WriteTagMetaData(int id)
        {
            this.WriteToTagInfo(id, 0);
            this.WriteToTagInfoShort(Screen.width, 8);
            this.WriteToTagInfoShort(Screen.height, 10);
            Profiler.EmitFrameMetaData(ScreenShotToProfiler.MetadataGuid, ScreenShotToProfiler.InfoTag, tagInfo);
        }
    }



}

#endif