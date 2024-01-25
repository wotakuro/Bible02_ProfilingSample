// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "stdlib.h"
#include "UnityPluginHeaders/IUnityProfilerCallbacks.h"
#include <iostream>
#include <fstream>
#include <mutex>

// 関数宣言処理
static void UNITY_INTERFACE_API SetupCreateMarkerCallback(const UnityProfilerMarkerDesc* markerDesc, void* userData);
static void UNITY_INTERFACE_API OnProfilerEvent(const UnityProfilerMarkerDesc* markerDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData);
static void WriteShaderCompileGPUProgramToBuffer(const UnityProfilerMarkerData* eventData);
static void WriteShaderCreateGPUProgramToBuffer(const UnityProfilerMarkerData* eventData);

// From PlatformDependSource
long GetThreadId();

static const int BufferSize = 1024 * 1024;

// Pluginのコールバック登録用のオブジェクトです
static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;

// 実行するかのフラグ
static bool s_executeFlag = false;

// Shaderコンパイル情報を書き込むBuffer
static char* buffer = NULL;
static int currentBufferIdx = 0;

// 複数Threadからbufferへの同時アクセスを想定して、Mutexでロックします
static std::mutex mtx;


//プラグインロード時の処理
static bool s_IsLoadedPlugin = false;

// 1.Pluginのロード時に呼び出されます
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces * unityInterfaces)
{
    if (!s_IsLoadedPlugin) {
        // Profilerからのコールバック登録用のIntefaceを取得します
        s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
        // 新しい名前のProfilerMakerが出来るたびに呼び出すコールバックを登録します
        s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
        s_IsLoadedPlugin = true;
    }
}
// 2.新しい名前のProfilerMakerが作成されるたびに呼び出されるコールバックです
static void UNITY_INTERFACE_API SetupCreateMarkerCallback(const UnityProfilerMarkerDesc* markerDesc, void* userData)
{
    // “Shader.CompileGPUProgram”と "Shader.CreateGPUProgram"で始まるProfilerMakerのみを対象とします
    if (strncmp(markerDesc->name, "Shader.CompileGPUProgram", 24) == 0 ||
        strncmp(markerDesc->name, "Shader.CreateGPUProgram", 23) == 0)
    {
        // ProfilerMaker.Begin/Endが呼び出されるタイミングでのコールバックを指定します
        s_UnityProfilerCallbacks->RegisterMarkerEventCallback(markerDesc, OnProfilerEvent, NULL);
    }
}

// 3.ProfilerMakerのBegin呼び出し時、End呼び出し時にコールバックされる部分です
static void UNITY_INTERFACE_API OnProfilerEvent(const UnityProfilerMarkerDesc* markerDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData)
{
    // 実行フラグが立っていないなら処理しません
    if (!s_executeFlag) { return; }

    // ProfilerMakerのイベントタイプで処理を分けます
    switch (eventType)
    {
        // ProfilerMaker.Begin時のイベントです

        case kUnityProfilerMarkerEventTypeBegin:
        {
            // Shader.CompileGPUProgramのときの処理です
            if (eventDataCount == 3 &&
                strncmp(markerDesc->name, "Shader.CompileGPUProgram", 24) == 0)
            {
                WriteShaderCompileGPUProgramToBuffer(eventData);
            }
            // Shader.CreateGPUProgramのときの処理です
            else if (eventDataCount == 4 &&
                strncmp(markerDesc->name, "Shader.CreateGPUProgram", 23) == 0)
            {
                WriteShaderCreateGPUProgramToBuffer(eventData);
            }

            break;
        }
        // ProfilerMaker.End時のイベントです
        case kUnityProfilerMarkerEventTypeEnd:
            break;

    }
}


// 4.(破棄処理周り)Pluginがアンロードされた時に呼び出されます
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    if (s_IsLoadedPlugin) {
        // 新しい名前のProfilerMakerが出来るたびに呼び出されるコールバックを解除
        s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
        // ProfilerMakerのBegin/Endのコールバックを解除
        s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, &OnProfilerEvent, NULL);
        s_IsLoadedPlugin = false;
    }
    // Bufferを破棄します
    if (buffer) {
        free(buffer);
        buffer = NULL;
    }
}

// Shader.CompileGPUProgramのときの処理です
static void WriteShaderCompileGPUProgramToBuffer(const UnityProfilerMarkerData* eventData) {

    mtx.lock();

    int size = BufferSize - currentBufferIdx - 1;

    // Bufferにこれ以上書き込めないので終了します
    if (size <= 0) {
        mtx.unlock();
        return;
    }

    // eventDataにはmetadataが入っています
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

// Shader.CreateGPUProgramのときの処理です
static void WriteShaderCreateGPUProgramToBuffer(const UnityProfilerMarkerData* eventData) {
    mtx.lock();
    int size = BufferSize - currentBufferIdx - 1;

    // Bufferにこれ以上書き込めないので終了します
    if (size <= 0) {
        mtx.unlock();
        return;
    }

    // eventDataにはmetadataが入っています
    // [0] shader name
    // [1] pass
    // [2] stage
    // [3] keyword
    int next = snprintf(buffer + currentBufferIdx, size, "%s,%s,%s,%s\n",
        reinterpret_cast<const char*>(eventData[0].ptr),
        reinterpret_cast<const char*>(eventData[1].ptr),
        reinterpret_cast<const char*>(eventData[2].ptr),
        reinterpret_cast<const char*>(eventData[3].ptr));
    currentBufferIdx += next;
    mtx.unlock();
}


//---------------- Unity C#から呼び出される処理 --------------------//

// NativePlugin内でBufferからのコピーなどを行う更新処理
extern "C" UNITY_INTERFACE_EXPORT const char* _NativeProfilerCallbackPluginUpdate() {
    // Mutexのロックをします
    mtx.lock();
    // 特にBufferへの書き込みが無かったなら何もしないで終了します
    if (currentBufferIdx <= 0) {
        mtx.unlock();
        return NULL;
    }
    // Bufferに書き込まれた分メモリを確保して、Bufferの内容をコピーします
    void* ptr = malloc(currentBufferIdx + 1);
    if (ptr) {
        memcpy(ptr, buffer, currentBufferIdx + 1);
    }
    currentBufferIdx = 0;
    mtx.unlock();
    // 文字列を返します（この ptrは勝手に解放されます)
    return reinterpret_cast<char*>(ptr);
}

// NativePlugin内でのバッファーセットアップ処理
extern "C" void UNITY_INTERFACE_EXPORT  _NativeProfilerCallbackPluginSetupBuffer()
{
    if (!buffer) {
        buffer = reinterpret_cast<char*>(malloc(BufferSize));
    }
}

// ShaderCompile情報を収集するかのフラグ
extern "C" void UNITY_INTERFACE_EXPORT _NativeProfilerCallbackPluginSetEnable(bool enable)
{
    s_executeFlag = enable;
}
