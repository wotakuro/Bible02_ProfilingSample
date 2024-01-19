using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundle
{
    [MenuItem("07_NativePluginSample/BuildAssetBundle")]
    public static void Execute()
    {
        string streamingAssetsPath = "Assets/StreamingAssets";
        if(!Directory.Exists(streamingAssetsPath) ){
            Directory.CreateDirectory(streamingAssetsPath);
        }
        var builds = new AssetBundleBuild[]
        {
            new AssetBundleBuild(){
                assetBundleName = "unitychan.ab",
                assetNames = new string[] {"Assets/UnityChan/Prefabs/unitychan.prefab"}
            }
        };
        BuildTarget target = BuildTarget.StandaloneWindows64;
#if UNITY_EDITOR_WIN
        target = BuildTarget.StandaloneWindows64;
#elif UNITY_EDITOR_OSX
        target = BuildTarget.StandaloneOSX;
#endif
        BuildPipeline.BuildAssetBundles(streamingAssetsPath, builds,
            BuildAssetBundleOptions.None, target );
    }
}
