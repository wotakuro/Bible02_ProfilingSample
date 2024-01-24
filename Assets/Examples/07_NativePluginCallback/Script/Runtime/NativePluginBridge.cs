using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

// NativePluginで実装されたProfilerCallbackから情報を得て
// 表示するブリッジ用のクラス
public class NativePluginBridge : MonoBehaviour
{
    // ネイティブプラグイン名
    const string dllName = "ProfilerCallbackNativePluginSample";

    [SerializeField]
    private Text info;
    [SerializeField]
    private ScrollRect scrollRect;

    #region NATIVE_PLUGIN

    // NativePlugin側のBufferをセットアップします
    [DllImport(dllName)]
    public static extern void _NativeProfilerCallbackPluginSetupBuffer();

    // NativePlugin側の更新をします。
    // Shaderコンパイルが走っていた場合、返り値のStringに文字列が返ってきます
    [DllImport(dllName)]
    public static extern string _NativeProfilerCallbackPluginUpdate();

    // NativePlugin側の有効・無効を切り替えます
    [DllImport(dllName)]
    public static extern void _NativeProfilerCallbackPluginSetEnable(bool enable);
    #endregion NATIVE_PLUGIN

    // Start処理
    void Start()
    {
        NativePluginBridge._NativeProfilerCallbackPluginSetupBuffer();
        NativePluginBridge._NativeProfilerCallbackPluginSetEnable(true);
    }

    // 破棄時の処理
    private void OnDestroy()
    {
        NativePluginBridge._NativeProfilerCallbackPluginUpdate();
        NativePluginBridge._NativeProfilerCallbackPluginSetEnable(false);
    }

    // Update処理
    void Update()
    {
        // NativePluginの側で更新処理をします
        var data = NativePluginBridge._NativeProfilerCallbackPluginUpdate();
        // Shaderコンパイルがある場合文字列が返ってきます
        if (!string.IsNullOrEmpty(data))
        {
            info.text += "------Frame:" + Time.frameCount + "---------\n" + data;
            // スクロールのサイズ更新をします
            if (scrollRect && info)
            {
                scrollRect.content.sizeDelta = new Vector2(info.preferredWidth, info.preferredHeight);
            }
        }

    }

}
