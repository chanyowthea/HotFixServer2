using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HotFix
{
    public class Util
    {
        static public string GetLocalPathByPlatfrom(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return string.Empty;
            }
            else
            {
                string localPath;
                // 如果从FileInfo中的文档读取的字段是Local,那么从StreamingAssets/win/AssetBundles/中加载
                if (fileInfo.State == HotFix.FileInfo.OpState.Local)
                {
#if UNITY_EDITOR
                    localPath = Context.LocalAddr.Replace("file://", "") + "/" + Context.AssetBundlePrefix + "/" + fileInfo.FullName;
#elif UNITY_ANDROID
                    localPath = Context.LocalAddr.Replace(Application.streamingAssetsPath, Application.dataPath + "!assets") + "/" + 
                        Context.AssetBundlePrefix + "/" + fileInfo.FullName;
#elif UNITY_IPHONE
					localPath = Context.LocalAddr.Replace("file://", "") + "/" + Context.AssetBundlePrefix + "/" + fileInfo.FullName;
#else
                    localPath = Context.LocalAddr + "/" + Context.AssetBundlePrefix + "/" + fileInfo.FullName;
#endif
                }
                else
                {
                    localPath = Context.CacheAddr + "/" + Context.AssetBundlePrefix + "/" + fileInfo.FullName;
                }
                return localPath;
            }
        }

        // 保存AssetBundle,AssetBundle.manifest,AssetBundle等文件
        static public bool SaveFile(string path, byte[] content)
        {
            try
            {
                string outputFullPath = Path.Combine(Context.CacheAddr, path);
                string outputDirectory = Path.GetDirectoryName(outputFullPath);
                if (Directory.Exists(outputDirectory) == false)
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                using (FileStream s = File.Create(outputFullPath))
                {
                    s.Write(content, 0, content.Length);
                }
                Debug.Log("save file successfully " + outputFullPath);
            }
            catch
            {
                return false;
            }
            return true;
        }

        // Util.GetLocalPathByPlatfrom(path, GetLocalFileInfo(path))
        static public AssetBundle LoadAssetBundle(string localPath)
        {
            if (string.IsNullOrEmpty(localPath))
            {
                return null;
            }
            return AssetBundle.LoadFromFile(localPath);
        }

        /*public bool LoadAssetBundleAsync(string path, Action<AssetBundle> finishedAction)
        {
            if(ResHotUpdaterContext.CoroutineHolder == null)
            {
                Debug.LogError("no coroutine holder found");
                return false;
            }
            string localPath = GetLocalPathByPlatfrom(path);
            if (string.IsNullOrEmpty(localPath))
            {
                return false;
            }
            ResHotUpdaterContext.CoroutineHolder.StartCoroutine(OnLoadAssetBundleAsync(localPath, finishedAction));
            return true;
        }
        IEnumerator OnLoadAssetBundleAsync(string path, Action<AssetBundle> finishedAction)
        {
            AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
            yield return bundleLoadRequest;
            AssetBundle abAsset = bundleLoadRequest.assetBundle;
            if (abAsset == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                finishedAction(null);
                yield break;
            }
            finishedAction(abAsset);
        }*/

    }
}
