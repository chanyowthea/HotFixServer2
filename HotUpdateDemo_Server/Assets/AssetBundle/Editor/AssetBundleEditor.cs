using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleEditor
{
    [MenuItem("Tools/CopyLuaToBytes")]
    static void CopyLuaToBytes()
    {
        string luaDir = Application.dataPath + "/AssetBundle/Lua/";
        string destDir = Application.dataPath + "/AssetBundle/Bytes/";
        var luaFiles = Recursive(luaDir);
        for (int i = 0, length = luaFiles.Count; i < length; i++)
        {
            string f = luaFiles[i];
            //var destPath = destDir + f.Substring(luaDir.Length); 
            //destPath = destPath.Substring(0, destPath.LastIndexOf('.')) + ".bytes";
            var destPath = destDir + f.Substring(luaDir.Length) + ".bytes";
            Debug.Log(destPath);
            if (File.Exists(destPath))
            {
                File.Delete(destPath); 
            }
            File.Copy(f, destPath);

            // 设置assetbundle name
            string s = destPath.Substring(Application.dataPath.Length + 1 - "Assets/".Length);
            var asset = AssetImporter.GetAtPath(s); 
            if (asset == null)
            {
                continue; 
            }
            asset.assetBundleName = "lua"; 
        }
        AssetDatabase.Refresh(); 
    }

    [MenuItem("Tools/BuildBundles")]
    static void BuildAllAssetBundles()
    {
        BuildAssetBundles(BuildTarget.StandaloneWindows64);
        BuildAssetBundles(BuildTarget.Android);
        //BuildAssetBundles(BuildTarget.iOS);
    }

    static void BuildAssetBundles(BuildTarget target)
    {
        //第一个参数获取的是AssetBundle存放的相对地址
#if SERVER
        string path = Application.streamingAssetsPath + "/" + GetDirByBuildTarget(target) +
            "/" + Application.version + "/" + HotFix.Context.AssetBundlePrefix + "/";
#else
        string path = Application.streamingAssetsPath + "/" + GetDirByBuildTarget(target) + 
            "/" + HotFix.Context.AssetBundlePrefix + "/";
#endif
        Debug.Log("path=" + path);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true); 
        }
        Directory.CreateDirectory(path);

        BuildPipeline.BuildAssetBundles(
         path,
         BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle,
         target);

        var list = Recursive(path);
        for (int i = 0, length = list.Count; i < length; i++)
        {
            var tempName = list[i];
            if (Path.GetExtension(tempName) != ".manifest")
            {
                FileInfo info = new FileInfo(tempName);
                string destPath = tempName + HotFix.Context._assetBundleSuffix;
                if (File.Exists(destPath))
                {
                    File.Delete(destPath); 
                }
                info.MoveTo(destPath);
            }
        }

#if SERVER
        FileUtil.WriteFileInfo(GetDirByBuildTarget(target));
#endif
        AssetDatabase.Refresh(); 
    }

    static List<string> Recursive(string path)
    {
        List<string> files = new List<string>();

        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);

        // 获取本目录子一级文件
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }

        // 遍历子二级文件夹
        foreach (string dir in dirs)
        {
            var l = Recursive(dir);
            if (l.Count > 0)
            {
                files.AddRange(l);
            }
        }
        return files;
    }

    static public string GetDirByBuildTarget(BuildTarget target)
    {
        if (target == BuildTarget.Android)
        {
            return "android";
        }
        else if (target == BuildTarget.iOS)
        {
            return "ios";
        }
        else if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
        {
            return "win";
        }
        else
        {
            return "unsupported";
        }
    }

    //[MenuItem("AssetBundle Editor/SetAssetBundleName")]
    //static void SetResourcesAssetBundleName()
    //{
    //    string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length); 
    //    //只读取当前选中的目录，排除子目录
    //    Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.Assets | SelectionMode.ExcludePrefab);
    //    //此处添加需要命名的资源后缀名,注意大小写。
    //    string[] Filtersuffix = new string[] { ".prefab", ".mat", ".bytes" };
    //    if (SelectedAsset.Length == 0) return;
    //    foreach (Object tmpFolder in SelectedAsset)
    //    {
    //        string fullPath = projectPath + AssetDatabase.GetAssetPath(tmpFolder);
    //        Debug.Log(fullPath);
    //        if (Directory.Exists(fullPath))
    //        {
    //            DirectoryInfo dir = new DirectoryInfo(fullPath);
    //            var files = dir.GetFiles("*", SearchOption.AllDirectories);
    //            for (var i = 0; i < files.Length; ++i)
    //            {
    //                var fileInfo = files[i];
    //                //显示进度条
    //                EditorUtility.DisplayProgressBar("设置AssetBundleName名称", "正在设置AssetBundleName名称中...", 1.0f * i / files.Length);
    //                foreach (string suffix in Filtersuffix)
    //                {
    //                    if (fileInfo.Name.EndsWith(suffix))
    //                    {
    //                        string path = fileInfo.FullName.Replace('\\', '/').Substring(projectPath.Length);
    //                        //资源导入器
    //                        var importer = AssetImporter.GetAtPath(path);
    //                        if (importer)
    //                        {
    //                            string name = path.Substring(fullPath.Substring(projectPath.Length).Length);
    //                            importer.assetBundleName = name.Substring(1, name.LastIndexOf('.') - 1);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    //删除所有未使用的assetBundle资产数据库名称
    //    AssetDatabase.RemoveUnusedAssetBundleNames();
    //    EditorUtility.ClearProgressBar();
    //}
}
