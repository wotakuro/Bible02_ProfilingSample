using UnityEngine;
using Unity.Profiling;

public class ProfilerMakerSample : MonoBehaviour
{
    static readonly ProfilerMarker profilerMarker = new ProfilerMarker("サンプルコードの負荷");

    // Update is called once per frame
    void Update()
    {
        using(profilerMarker.Auto())
        {
            System.Threading.Thread.Sleep(10);
        }
    }
}
