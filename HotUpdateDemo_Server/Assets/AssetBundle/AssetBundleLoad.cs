using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 读取Assetbundle存储目录的AssetBundle文件
// 读取Assetbundle存储目录的Manifest文件
// 加载目标AssetBundle的依赖项
// 加载目标AssetBundle

public class AssetBundleLoad : MonoBehaviour
{
    private void Start()
    {

    }

    void OnGUI()
    {
        if (GUILayout.Button("Load", GUILayout.Width(300), GUILayout.Height(200)))
        {
            LoadManifest();
            var go = InstanceAsset("bot");
            go.transform.localEulerAngles = new Vector3(-10, -50, 17); 
            //var s = manifest.GetAssetBundleHash("jushiguai.assetbundle");
            //Debug.Log(s);
        }
    }

    // name不需要后缀,加载路径需要后缀
    private static AssetBundleManifest manifest = null;
    private static Dictionary<string, AssetBundle> assetBundleDic = new Dictionary<string, AssetBundle>();

    void LoadManifest()
    {
        // 加载StreamingAssets的AssetBundle
        var path = HotFix.Util.GetLocalPathByPlatfrom(GCommon.ResHotUpdater.instance.GetLocalFileInfo(HotFix.Context.AssetBundlePrefix + HotFix.Context._assetBundleSuffix));
        AssetBundle manifestAssetBundle = AssetBundle.LoadFromFile(path);
        // 加载AssetBundleManifest
        manifest = (AssetBundleManifest)manifestAssetBundle.LoadAsset("AssetBundleManifest");
    }

    public AssetBundle LoadAssetBundle(string Url)
    {
        // 如果这个字典里有，那么加载字典中的AssetBundle
        if (assetBundleDic.ContainsKey(Url))
            return assetBundleDic[Url];

        if (manifest != null)
        {
            //获取当前加载AssetBundle的所有依赖项的路径
            string[] objectDependUrl = manifest.GetAllDependencies(Url);
            foreach (string tmpUrl in objectDependUrl)
            {
                //通过递归调用加载所有依赖项
                LoadAssetBundle(tmpUrl);
            }

            var path = HotFix.Util.GetLocalPathByPlatfrom(GCommon.ResHotUpdater.instance.GetLocalFileInfo(Url)); 
            Debug.Log("LoadAssetBundle " + path);
            assetBundleDic[Url] = AssetBundle.LoadFromFile(path);
            return assetBundleDic[Url];
        }
        return null;
    }

    GameObject InstanceAsset(string assetBundleName)
    {
        string assetBundlePath = assetBundleName;
        int index = assetBundleName.LastIndexOf('/');
        string realName = assetBundleName.Substring(index + 1, assetBundleName.Length - index - 1);
        var bundle = LoadAssetBundle(assetBundlePath);
        if (bundle != null)
        {
            Object tmpObj = bundle.LoadAsset(realName);
            var go = GameObject.Instantiate(tmpObj);
            bundle.Unload(false);
            return (GameObject)go; 
        }
        return null; 
    }
}
