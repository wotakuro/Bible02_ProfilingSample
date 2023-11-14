// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "stdlib.h"
#include "UnityPluginHeaders/IUnityProfilerCallbacks.h"
#include <iostream>
#include <fstream>
#include <mutex>

// From PlatformDependSource
long GetThreadId();

static const int BufferSize = 1024 * 1024;
static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;

static bool s_executeFlag = false;
static char* buffer = NULL;
static int currentBufferIdx = 0;

static std::mutex mtx;

static void WriteFileShaderCompileGPUProgram(const UnityProfilerMarkerData* eventData) {

    mtx.lock();

    int size = BufferSize - currentBufferIdx -1;

    // [0] shader name
    // [1] pass
    // [2] keyword
    int next = snprintf(buffer + currentBufferIdx, size, "%s,EditorCompile,%s,%s\n",
        reinterpret_cast<const char*>(eventData[0].ptr),
        reinterpret_cast<const char*>(eventData[1].ptr),
        reinterpret_cast<const char*>(eventData[2].ptr));
    currentBufferIdx += next;
    mtx.unlock();
}

static void WriteFileShaderCreateGPUProgram(const UnityProfilerMarkerData* eventData) {
    mtx.lock();
    int size = 1024;
    // [0] shader name
    // [1] pass
    // [2] stage
    // [3] keyword
    int next = snprintf(buffer + currentBufferIdx,size, "%s,%s,%s,%s\n",
        reinterpret_cast<const char*>(eventData[0].ptr),
        reinterpret_cast<const char*>(eventData[1].ptr),
        reinterpret_cast<const char*>(eventData[2].ptr),
        reinterpret_cast<const char*>(eventData[3].ptr) );
    currentBufferIdx += next;
    mtx.unlock();
}

static void UNITY_INTERFACE_API OnProfilerEvent(const UnityProfilerMarkerDesc* markerDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData)
{
    if (!s_executeFlag) { return; }

    switch (eventType)
    {
    case kUnityProfilerMarkerEventTypeBegin:
    {

        if (eventDataCount == 3 &&
            strncmp(markerDesc->name, "Shader.CompileGPUProgram", 24) == 0)
        {
            WriteFileShaderCompileGPUProgram(eventData);
        }else if (eventDataCount == 4 &&
            strncmp(markerDesc->name, "Shader.CreateGPUProgram", 23) == 0)
        {
            WriteFileShaderCreateGPUProgram(eventData);
        }

        break;
    }
    case kUnityProfilerMarkerEventTypeEnd:
        break;

    }
}

extern "C" UNITY_INTERFACE_EXPORT const char* _NativeProfilerCallbackPluginUpdate() {

    mtx.lock();
    if (currentBufferIdx <= 0) {
        mtx.unlock();
        return NULL;
    }
    void* ptr = malloc(currentBufferIdx+1);
    if (ptr) {
        memcpy(ptr, buffer, currentBufferIdx + 1);
    }
    currentBufferIdx = 0;
    mtx.unlock();
    return reinterpret_cast<char*>(ptr);
}


extern "C" void UNITY_INTERFACE_EXPORT  _NativeProfilerCallbackPluginSetupBuffer()
{
    if (!buffer) {
        buffer = reinterpret_cast<char*>(malloc(BufferSize));
    }
}


extern "C" void UNITY_INTERFACE_EXPORT _NativeProfilerCallbackPluginSetEnable(bool enable)
{
    s_executeFlag = enable;
}



static void UNITY_INTERFACE_API SetupCreateMarkerCallback(const UnityProfilerMarkerDesc* markerDesc, void* userData)
{
    if( strncmp(markerDesc->name, "Shader.CompileGPUProgram", 24) == 0 ||
        strncmp(markerDesc->name, "Shader.CreateGPUProgram", 23) == 0 )
        {
            s_UnityProfilerCallbacks->RegisterMarkerEventCallback(markerDesc, OnProfilerEvent, NULL);
        }
}

static bool s_IsLoadedPlugin = false;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces * unityInterfaces)
{
    if (!s_IsLoadedPlugin) {
        s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
        s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
        s_IsLoadedPlugin = true;
    }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    if (s_IsLoadedPlugin) {
        s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
        s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, &OnProfilerEvent, NULL);
        s_IsLoadedPlugin = false;
    }
    if (buffer) {
        free(buffer);
        buffer = NULL;
    }
}

