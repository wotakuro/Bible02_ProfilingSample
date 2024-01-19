using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;


public class NativePluginBridge : MonoBehaviour
{
    const string dllName = "ProfilerCallbackNativePluginSample";

    [SerializeField]
    private Text info;
    [SerializeField]
    private ScrollRect scrollRect;


    [DllImport(dllName)]
    public static extern void _NativeProfilerCallbackPluginSetupBuffer();

    [DllImport(dllName)]
    public static extern string _NativeProfilerCallbackPluginUpdate();


    [DllImport(dllName)]
    public static extern void _NativeProfilerCallbackPluginSetEnable(bool enable);

    // Start is called before the first frame update
    void Start()
    {
        NativePluginBridge._NativeProfilerCallbackPluginSetupBuffer();
        NativePluginBridge._NativeProfilerCallbackPluginSetEnable(true);
    }

    private void OnDestroy()
    {
        NativePluginBridge._NativeProfilerCallbackPluginUpdate();
        NativePluginBridge._NativeProfilerCallbackPluginSetEnable(false);
    }

    // Update is called once per frame
    void Update()
    {
        var data = NativePluginBridge._NativeProfilerCallbackPluginUpdate();
        if (!string.IsNullOrEmpty(data))
        {
            info.text += "------Frame:" + Time.frameCount + "---------\n" + data;
            if (scrollRect && info)
            {
                scrollRect.content.sizeDelta = new Vector2(info.preferredWidth, info.preferredHeight);
            }
        }

    }

}
