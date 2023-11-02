using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AssetBundleLoader : MonoBehaviour
{
    private AssetBundle assetBundle;
    // Start is called before the first frame update
    void Start()
    {
        assetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        if(assetBundle == null)
        {
            Debug.LogError("AssetBundle‚ªƒ[ƒh‚Å‚«‚Ü‚¹‚ñ");
        }
    }

    private string AssetBundlePath
    {
        get
        {
            return Path.Combine(Application.streamingAssetsPath, "unitychan.ab");
        }
    }

    private void OnDestroy()
    {
        assetBundle.Unload(true);
    }
}
