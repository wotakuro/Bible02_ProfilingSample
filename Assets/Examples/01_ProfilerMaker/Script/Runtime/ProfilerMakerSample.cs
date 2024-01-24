using UnityEngine;
using Unity.Profiling;

// ProfilerMakerでProfilerに指定区域の情報を追加するサンプルです
public class ProfilerMakerSample : MonoBehaviour
{
    // 計測用のProfilerMakerを作成します
    static readonly ProfilerMarker profilerMarker = new ProfilerMarker("サンプルコードの負荷");

    // Update処理
    void Update()
    {
        // 下記で括られた区間をProfilerに載せます
        // 開始時にProfilerMaker.Begin(); 終了時にProfilerMaker.End();と書いても同等です
        // しかし、Begin/Endを対で書かないとエラーを出します。
        // エラーを防ぐためにも using( Auto() )と書いています
        using(profilerMarker.Auto())
        {
            // 処理するかわりに適当にSleepさせています
            System.Threading.Thread.Sleep(10);
        }
    }
}
