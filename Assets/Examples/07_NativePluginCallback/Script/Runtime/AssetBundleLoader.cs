using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AssetBundleLoader : MonoBehaviour
{
    private AssetBundle assetBundle;
    private List<GameObject> gmos = new List<GameObject>();

    [SerializeField]
    private GameObject warningText;


    private void Awake()
    {
        if (warningText)
        {
            warningText.SetActive( !File.Exists(this.AssetBundlePath));
        }
    }

    // Start is called before the first frame update
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
            gmos.Add( gmo );
        }
    }
    public void Unload()
    {
        foreach(var gmo in gmos)
        {
            GameObject.Destroy(gmo);
        }
        gmos.Clear();
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
