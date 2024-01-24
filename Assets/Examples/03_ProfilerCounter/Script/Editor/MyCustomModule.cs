using System;
using Unity.Profiling;
using Unity.Profiling.Editor;

// ProfilerModuleを継承したクラスを作成することで、独自のModuleを作成できます
// ProfilerCounterSampleに対応したModuleを作ります
[Serializable]
[ProfilerModuleMetadata("MyCustomModule")]
public class MyCustomModule : ProfilerModule
{
    // チャートに表示するカウンターを配列で宣言します
    static readonly ProfilerCounterDescriptor[] k_ChartCounters =
        new ProfilerCounterDescriptor[]{
        new ProfilerCounterDescriptor("キャラの数", ProfilerCategory.Scripts),
    };
    // コンストラクタで、baseクラスにカウンター一覧を渡して初期化します
    public MyCustomModule() : base(k_ChartCounters) { }
}

