using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AssetBundleLoader : MonoBehaviour
{
    private AssetBundle assetBundle;
    private List<GameObject> gmos = new List<GameObject>();


    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    public void Load()
    {
        if (assetBundle != null) { return; }
        assetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        if(assetBundle == null)
        {
            Debug.LogError("AssetBundleÇ™ÉçÅ[ÉhÇ≈Ç´Ç‹ÇπÇÒ");
        }
        var prefabs = assetBundle.LoadAllAssets<GameObject>();
        foreach( var prefab in prefabs )
        {
            var gmo = GameObject.Instantiate(prefab);
            gmos.Add( gmo );
        }
    }
    public void Unload()
    {
        if (assetBundle)
        {
            assetBundle.Unload(true);
        }
    }

    private void OnDestroy()
    {
        Unload();
    }

    private string AssetBundlePath
    {
        get
        {
            return Path.Combine(Application.streamingAssetsPath, "unitychan.ab");
        }
    }

}
