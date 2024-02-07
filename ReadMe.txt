===========================================
「Unityバイブル R6冬号」サンプルプログラム

「Unity Profiler」
===========================================

　これは、株式会社ボーンデジタル社 発行・発売の「Unityバイブル R6冬号」の Timeline のセクションのサンプルプロジェクトです。

■サンプルプロジェクトについて
　サンプルプロジェクトには、1つのプロジェクト内に複数のサンプルケースが入っています。
　各サンプルは、「Assets/Examples以下にあります」

■フォルダ／ファイルの説明
・Assets/Examples以下
 - Assets/Examples/01_ProfilerMaker
　「ProfilerMarkerで指定区間処理負荷を計測」の実際のサンプルです
 - Assets/Examples/02_ProfilerMakerWithMetadata
　「ProfilerMakerを用いたMetadata埋め込み」の実際のサンプルです
 - Assets/Examples/03_ProfilerCounter
　「ProfilerCounterによるカウンターの追加」の実際のサンプルです
 - Assets/Examples/04_ProfilerRecorder
　「ProfilerRecorderを利用した処理時間の負荷計測」の実際のサンプルです
 - Assets/Examples/05_GettingCounter
　「ProfilerRecorderでMemory使用量やDrawCallの計測」の実際のサンプルです
 - Assets/Examples/06_RelseaseBuildProfiler
　「ReleaseBuildでも取れるデータ」の実際のサンプルです
 - Assets/Examples/07_NativePluginCallBack(Windows/Macのみ)
　「Native Pluginで ProfilerからCallbackを登録する方法」の実際のサンプルです
　フォルダ内にある「NativePluginSoufrce~」以下に、C＋＋で記述されたネイティブプラグインソースを含みます
 - Assets/Examples/08_GettingProfilerDataInEditor (Editor拡張のみ)
　「ProfilerDriverを利用したProfilerWindowのデータアクセス」の実際のサンプルです
 - Assets/Examples/09_GettingRawFrameDataInEditor (Editor拡張のみ)
　「RawFrameDataViewを用いたProfilerMakerのデータ取得」の実際のサンプルです
 - Assets/Examples/10_ObjectInstantiateListInEditor (Editor拡張のみ)
　「GetUnityObjectInfoで生成したオブジェクトの情報を取得」の実際のサンプルです
 - Assets/Examples/11_FrameMetaData (Runtime+Editor)
　「EmitFrameMetaDataでデータの埋め込みGetFrameMetaDataで取得」の実際のサンプルです
 - Assets/Examples/Common
　複数のサンプルケースで利用している共通コードが入っています。

・Readme.txt
　このファイルです。

・appendix
 - appendix/RenderingGpuWatcher.cs
　URPでのGPUの処理時間を画面に表示するおまけファイルです。
　同様のものを下記URLで配布しています
　https://gist.github.com/wotakuro/b831eec78d919fd3787ca277c16ade95
 - appendix/ScreenshotToUnityProfiler
　ScreenshotをUnityProfilerに乗せるおまけファイルです。
　同様のものを下記URLで配布しています
　https://github.com/wotakuro/ScreenshotToUnityProfiler

■ライセンス表記
・サンプルには「ユニティ・テクノロジーズ・ジャパン株式会社」提供のUnityChan及びUnityChanToonShader Version2を使用しています

■サンプルプロジェクトの利用上の注意
　提供しているデータはUnityの学習のために作成したもので、実用を保証するものではありません。

■サンプルプロジェクトのコードについて
ご自由にお使いください。

