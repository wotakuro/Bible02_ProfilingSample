using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class NativePluginBridge
{
    const string dllName = "ProfilerCallbackNativePluginSample";

    [DllImport(dllName)]
    public static extern void _NativeProfilerCallbackPluginSetupBuffer();

    [DllImport(dllName)]
    public static extern bool _NativeProfilerCallbackPluginUpdate();

    [DllImport(dllName)]
    public static extern string _NativeProfilerCallbackPluginGetUpdateResult();


    [DllImport(dllName)]
    public static extern void _NativeProfilerCallbackPluginSetEnable(bool enable);
}
