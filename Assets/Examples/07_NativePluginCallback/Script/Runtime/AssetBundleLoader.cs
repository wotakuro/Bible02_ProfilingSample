using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Shaderなどの動的ロードのためにAssetBundleをロードします
public class AssetBundleLoader : MonoBehaviour
{
    // ロードするAssetBundleオブジェクト
    private AssetBundle assetBundle;
    // AssetBundleからInstantiateしたオブジェクト一覧
    private List<GameObject> gameObjects = new List<GameObject>();

    // 警告用のテキスト表示オブジェクト
    [SerializeField]
    private GameObject warningText;


    private void Awake()
    {
        if (warningText)
        {
            warningText.SetActive( !File.Exists(this.AssetBundlePath));
        }
    }

    // AssetBUndleのロード処理
    public void Load()
    {
        if (assetBundle != null) { return; }
        assetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        if (assetBundle == null)
        {
            Debug.LogError("AssetBundleがロードできません");

            if (warningText)
            {
                warningText.SetActive(true);
            }
            return;
        }
        var prefabs = assetBundle.LoadAllAssets<GameObject>();
        foreach( var prefab in prefabs )
        {
            var gmo = GameObject.Instantiate(prefab);
            gameObjects.Add( gmo );
        }
    }

    // AssetBundleのUnload処理
    public void Unload()
    {
        foreach(var gmo in gameObjects)
        {
            GameObject.Destroy(gmo);
        }
        gameObjects.Clear();
        if (assetBundle)
        {
            assetBundle.Unload(true);
        }
    }

    // 破棄時の処理
    private void OnDestroy()
    {
        Unload();
    }

    // AssetBundleのパス取得
    private string AssetBundlePath
    {
        get
        {
            return Path.Combine(Application.streamingAssetsPath, "unitychan.ab");
        }
    }

}
