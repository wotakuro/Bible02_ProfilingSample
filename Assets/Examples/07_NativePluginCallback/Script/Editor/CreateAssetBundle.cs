using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundle
{
    [MenuItem("Example/07_NativePlugin_BuildAssetBundle")]
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
        BuildTarget target;
        target = BuildTarget.StandaloneWindows64;

        BuildPipeline.BuildAssetBundles(streamingAssetsPath, builds,
            BuildAssetBundleOptions.None, target );
    }
}
