using System;
using System.IO;
using UnityEngine;

namespace HotFix
{
    public enum ResHotUpdateResult
    {
        ERROR, COLDUPDATE, HOTUPDATE, PASS
    }

    public class Context
    {
        static public string RemoteAddr;
        static public string VersionInfoPath = "versioninfo";
        static public string FileInfoPath = "fileinfo";
        static public string AssetBundlePrefix = "AssetBundles";
        static public int MaxLoaderCountSimultaneously = 5;
        static public int MaxRetryCount = 5;

        static public string LocalAddr = Application.streamingAssetsPath + "/" + PlatformIdentifier;
        static public string CacheDir = Path.Combine(Application.persistentDataPath, "contentcache/");
        static public string CacheAddr = CacheDir + Context.PlatformIdentifier;
        static public string _localVersionInfoPath { get { return Path.Combine(LocalAddr, VersionInfoPath); } }
        static public string _cacheVersionInfoPath { get { return Path.Combine(CacheAddr, VersionInfoPath); } }

        static public string _localFileInfoPath { get { return Path.Combine(LocalAddr, FileInfoPath); } }
        static public string _cacheFileInfoPath { get { return Path.Combine(CacheAddr, FileInfoPath); } }
        static public bool NoResourceDownload = false;

        // 热更新结束
        static public Action<bool> OnHotUpdateFinished;
        static public MonoBehaviour CoroutineHolder;
        // 获取本地VersionInfo结束
        static public Action<ResHotUpdateResult> OnGetLocalVerionFinished;
        // 开始热更新
        static public Action<ResHotUpdateResult> OnStarted;

        static public string PlatformIdentifier
        {
            get
            {
#if UNITY_STANDALONE_WIN
                return "win";
#elif UNITY_IPHONE
                return "ios";
#elif UNITY_ANDROID
                return "android";
#else
                return "unsupported";
#endif
            }
        }

        static public string AssetBundleManifestPath
        {
            get
            {
                return Context.AssetBundlePrefix;
            }
        }

        public static string _assetBundlePath = LocalAddr + "/" + AssetBundlePrefix + "/";
        //AssetBundle打包的后缀名
        public static string _assetBundleSuffix = ".unity3d";
    }
}