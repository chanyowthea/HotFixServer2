using HotFix;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TsiU;
using UnityEngine;

namespace GCommon
{
    public class ResHotUpdater : TSingleton<ResHotUpdater>
    {
        private ResVersionInfo_Local m_LocalVersionInfo;
        private ResVersionInfo_Remote m_RemoteVersionInfo;
        private List<HotFix.FileInfo> m_NeedUpdateFiles;
        private ResFileLoaderController m_LoaderController;
        private ResUpdaterProgressMonitor m_UpdateProgressMonitor;
        public ResErrorCode LastError
        {
            get; set;
        }
        public string Version
        {
            get
            {
                if (m_LocalVersionInfo == null)
                {
                    return "1.0.0";
                }
                return m_LocalVersionInfo.GetVersionString();
            }
        }
        public void Init()
        {
            //set local res path
#if UNITY_ANDROID && !UNITY_EDITOR
            Context.LocalAddr = Application.streamingAssetsPath + "/" + Context.PlatformIdentifier;
#else
            Context.LocalAddr =
                //"file://" + 
                Application.streamingAssetsPath + "/" + Context.PlatformIdentifier;
            Debug.Log("ResHotUpdaterContext.LocalAddr=" + Context.LocalAddr);
#endif
            Context.CacheAddr = Path.Combine(Application.persistentDataPath, "contentcache/" + Context.PlatformIdentifier);
            Debug.Log("ResHotUpdaterContext.CacheAddr=" + Context.CacheAddr);
            Context.OnHotUpdateFinished = null;
            Context.MaxRetryCount = Mathf.Max(Context.MaxRetryCount, 5);
            m_LoaderController = new ResFileLoaderController(Context.MaxLoaderCountSimultaneously);
            m_UpdateProgressMonitor = new ResUpdaterProgressMonitor();
        }

        // 热更之前检查版本,获取本地版本信息
        public void StartGetLocalVersion()
        {
            //load version info 
            m_LocalVersionInfo = new ResVersionInfo_Local();

            if (!Directory.Exists(Context.CacheAddr))
            {
                Directory.CreateDirectory(Context.CacheAddr); 
            }
            // 从Local复制文件到Cache
            string cachePath = Path.Combine(Context.CacheAddr, Context.VersionInfoPath);
            if (!File.Exists(cachePath))
            {
                // 不用WWW,本地文件可以直接用File,应该会快一点
                string localPath = Path.Combine(Context.LocalAddr, Context.VersionInfoPath);
                if (!File.Exists(localPath))
                {
                    LastError = ResErrorCode.DownloadFailed;
                    onLocalVersionInfoLoaded(false);
                    Debug.LogError("文件不存在! path=" + localPath);
                }
                else
                {
                    File.Copy(localPath, cachePath);
                }
            }

            // 如果复制成功
            if (File.Exists(cachePath))
            {
                m_LocalVersionInfo.LoadVersionInfo((ResErrorCode code) => LastError = code,
                    Path.Combine(Context.CacheAddr, Context.VersionInfoPath), File.ReadAllText(cachePath), onLocalVersionInfoLoaded);
            }
        }

        // 如果跳过版本检查
        public void StartGetLocalFileInfo()
        {
            //otherwise, set pass state
            // 加载本地FileInfo,供后面加载AssetBundle使用
            // 其实可以不用,直接在需要加载AssetBundle之前读取一遍就行
            LoadFileInfo_Local(Context._cacheFileInfoPath, onLocalFileInfoLoaded_Pass);
        }

        // 检查版本更新
        public void StartVersionCheck(string remoteAddr, string remoteVersion = null)
        {
            //apply platform sub path
            Context.RemoteAddr = remoteAddr;
            if (Context.RemoteAddr[Context.RemoteAddr.Length - 1] != '/')
            {
                Context.RemoteAddr += @"/";
            }
            Context.RemoteAddr = Context.RemoteAddr + Context.PlatformIdentifier;
            if (Context.NoResourceDownload)
            {
                onRemoveVersionInfoLoaded(true);
            }
            else
            {
                m_RemoteVersionInfo = new ResVersionInfo_Remote();
                if (string.IsNullOrEmpty(remoteVersion))
                {
                    //get remote version from cdn server
                    // ResVersionInfo.Load
                    Action<WWW, ResFileLoader> onLoaded = (www, loader) =>
                    {
                        //error occurs
                        if (www == null || string.IsNullOrEmpty(www.error) == false)
                        {
                            LastError = ResErrorCode.DownloadFailed;
                            onRemoveVersionInfoLoaded(false);
                            return;
                        }
                        m_RemoteVersionInfo.LoadVersionInfo((ResErrorCode code) => LastError = code,
                            Path.Combine(Context.CacheAddr, Context.VersionInfoPath), www.text, onRemoveVersionInfoLoaded);
                    };
                    // 下载远端版本文件
                    LoadFromRemote(Context.VersionInfoPath, onLoaded, false);
                }
                else
                {
                    m_RemoteVersionInfo.LoadVersionInfo((ResErrorCode code) => LastError = code,
                        Path.Combine(Context.CacheAddr, Context.VersionInfoPath), remoteVersion, onRemoveVersionInfoLoaded);
                }
            }
        }
        public void StartHotUpdateDownload(Action<bool> onHotUpdateFinished)
        {
            Context.OnHotUpdateFinished = onHotUpdateFinished;
            if (m_NeedUpdateFiles == null || m_NeedUpdateFiles.Count == 0)
            {
                NotifyHotUpdateFinished(true);
                return;
            }
            Action<WWW, ResFileLoader> onLoaded = (www, loader) =>
            {
                if (www != null && www.error == null && www.bytes != null)
                {
                    m_UpdateProgressMonitor.OnLoadFinished((uint)www.size);
                    if (Util.SaveFile(loader.RelativePath, www.bytes) == false)
                    {
                        NotifyHotUpdateFinished(false, ResErrorCode.SaveFailed);
                    }
                }
                else
                {
                    if (loader.RetryCount > Context.MaxRetryCount)
                    {
                        NotifyHotUpdateFinished(false, ResErrorCode.DownloadFailed);
                    }
                    else
                    {
                        m_LoaderController.AddLoader(loader, true);
                        Debug.Log("retry file: " + loader.FullPath + ", " + loader.RetryCount);
                    }
                }
            };
            foreach (HotFix.FileInfo fileInfo in m_NeedUpdateFiles)
            {
                Debug.Log("file to update: " + fileInfo.Print());
                // 从远端下载AssetBundle
                LoadFromRemote(Context.AssetBundlePrefix + "/" + fileInfo.FullName, onLoaded, true);
            }
            m_NeedUpdateFiles.Clear();
        }
        public void Update(float time)
        {
            m_LoaderController.Update(time);
            if (m_LoaderController.IsFinished())
            {
                NotifyHotUpdateFinished(true);
            }
        }
        public void Clear()
        {
            if (m_LoaderController != null)
            {
                m_LoaderController.Clear();
            }
            if (m_UpdateProgressMonitor != null)
            {
                m_UpdateProgressMonitor.Clear();
            }
        }
        public void LoadFromRemote(string path, Action<WWW, ResFileLoader> onLoaded, bool includeVersionDir = true)
        {
            ResFileLoader loader = new ResFileLoader();
            if (includeVersionDir) //resource is in different version dir
            {
                loader.FullPath = Context.RemoteAddr + "/" + m_RemoteVersionInfo.GetVersionString() + "/" + path;
            }
            else
            {
                //only for version file
                loader.FullPath = Context.RemoteAddr + "/" + path;
            }
            loader.RelativePath = path;
            Debug.Log("loader.RelativePath=" + loader.RelativePath);
            loader.OnLoaded = onLoaded;
            m_LoaderController.AddLoader(loader);
        }


        #region Monitor
        public long GetTotalSizeInByte()
        {
            if (m_UpdateProgressMonitor != null)
            {
                return m_UpdateProgressMonitor.TotalSizeInByte;
            }
            return 0;
        }
        public long GetTotalLoadedSizeInByte()
        {
            if (m_UpdateProgressMonitor != null)
            {
                return m_UpdateProgressMonitor.TotalLoadedSizeInByte;
            }
            return 0;
        }
        #endregion

        public HotFix.FileInfo GetLocalFileInfo(string path)
        {
            HotFix.FileInfo fileInfo;
            if (m_LocalVersionInfo.m_FileInfos.TryGetValue(path, out fileInfo) == false)
            {
                return null;
            }
            return fileInfo;
        }

        private void NotifyHotUpdateFinished(bool result, ResErrorCode error = ResErrorCode.OK)
        {
            if (Context.OnHotUpdateFinished != null)
            {
                if (result == true)
                {
                    //save version info and file info
                    result = SaveVersionInfo();
                    if (result == false)
                    {
                        error = ResErrorCode.SaveFailed;
                    }
                }
                if (result == false)
                {
                    this.LastError = error;
                }
                Action<bool> hotUpdateFinishedCB = Context.OnHotUpdateFinished;
                Context.OnHotUpdateFinished = null;
                hotUpdateFinishedCB(result);
            }
        }
        private bool SaveVersionInfo()
        {
            return m_LocalVersionInfo.Save(Path.Combine(Context.CacheAddr, Context.VersionInfoPath),
                Path.Combine(Context.CacheAddr, Context.FileInfoPath));
        }

        //step 1
        private void onLocalVersionInfoLoaded(bool result)
        {
            Debug.Log("onLocalVersionInfoLoaded: " + result);
            if (result == false)
            {
                Context.OnGetLocalVerionFinished(ResHotUpdateResult.ERROR);
                return;
            }
            Context.OnGetLocalVerionFinished(ResHotUpdateResult.PASS);
        }
        //step 2
        private void onRemoveVersionInfoLoaded(bool result)
        {
            Debug.Log("onRemoveVersionInfoLoaded: " + result);
            if (result == false)
            {
                Context.OnStarted(ResHotUpdateResult.ERROR);
                return;
            }
            if (Context.NoResourceDownload == false)
            {
                if (m_LocalVersionInfo.MajorVersion < m_RemoteVersionInfo.MajorVersion)
                {
                    //cold update!!
                    Context.OnStarted(ResHotUpdateResult.COLDUPDATE);
                    return;
                }
                if (m_LocalVersionInfo.MajorVersion == m_RemoteVersionInfo.MajorVersion && m_LocalVersionInfo.MinorVersion < m_RemoteVersionInfo.MinorVersion)
                {
                    m_LocalVersionInfo.MajorVersion = m_RemoteVersionInfo.MajorVersion;
                    m_LocalVersionInfo.MinorVersion = m_RemoteVersionInfo.MinorVersion;
                    //hot update!! load file info and check detail file stats
                    Debug.Log("ResHotUpdater.ResHotUpdaterContext.FileInfoPath=" + Context.FileInfoPath);
                    LoadFileInfo_Local(Context._cacheFileInfoPath, onLocalFileInfoLoaded_Hotupdate);
                    return;
                }
            }
            //otherwise, set pass state
            LoadFileInfo_Local(Context._cacheFileInfoPath, onLocalFileInfoLoaded_Pass);
        }

        //step 3.1 if pass
        private void onLocalFileInfoLoaded_Pass(bool result)
        {
            Debug.Log("onLocalFileInfoLoaded_Pass: " + result);
            if (result == false)
            {
                Context.OnStarted(ResHotUpdateResult.ERROR);
                return;
            }
            Context.OnStarted(ResHotUpdateResult.PASS);
        }

        //step 3.2 if need hot update
        private void onLocalFileInfoLoaded_Hotupdate(bool result)
        {
            Debug.Log("onLocalFileInfoLoaded_Hotupdate: " + result);
            if (result == false)
            {
                Context.OnStarted(ResHotUpdateResult.ERROR);
                return;
            }
            LoadFileInfo_Remote(onRemoteFileInfoLoaded);
        }

        //step 4 if need hot update
        private void onRemoteFileInfoLoaded(bool result)
        {
            Debug.Log("onRemoteFileInfoLoaded: " + result);
            if (result == false)
            {
                Context.OnStarted(ResHotUpdateResult.ERROR);
                return;
            }
            //compare file info
            m_NeedUpdateFiles = new List<HotFix.FileInfo>();
            Dictionary<string, HotFix.FileInfo> localFileInfos = m_LocalVersionInfo.m_FileInfos;
            foreach (KeyValuePair<string, HotFix.FileInfo> v in m_RemoteVersionInfo.m_FileInfos)
            {
                HotFix.FileInfo localFileInfo;
                if (localFileInfos.TryGetValue(v.Key, out localFileInfo) == false)
                {
                    m_NeedUpdateFiles.Add(v.Value);
                    m_UpdateProgressMonitor.AddLoaderInfo(v.Value.Size);
                    //add into local
                    localFileInfo = v.Value.Clone();
                    localFileInfo.State = HotFix.FileInfo.OpState.Cache;
                    localFileInfos.Add(localFileInfo.FullName, localFileInfo);
                }
                else
                {
                    if (v.Value.Size != localFileInfo.Size || v.Value.Hash != localFileInfo.Hash)
                    {
                        m_NeedUpdateFiles.Add(v.Value);
                        m_UpdateProgressMonitor.AddLoaderInfo(v.Value.Size);
                        //update local
                        localFileInfo.Size = v.Value.Size;
                        localFileInfo.Hash = v.Value.Hash;
                        localFileInfo.State = HotFix.FileInfo.OpState.Cache;
                    }
                }
            }
            Context.OnStarted(ResHotUpdateResult.HOTUPDATE);
        }

        void LoadFileInfo_Local(string path, Action<bool> onFinish)
        {
            try
            {
                path = path.Replace('\\', '/');
                string s = File.ReadAllText(path);
                if (!m_LocalVersionInfo.ParseFileInfo(s))
                {
                    LastError = ResErrorCode.CorruptFile;
                    if (onFinish != null)
                    {
                        onFinish(false);
                    }
                }
                if (onFinish != null)
                {
                    onFinish(true);
                }
            }
            catch
            {
                LastError = ResErrorCode.DownloadFailed;
                if (onFinish != null)
                {
                    onFinish(false);
                }
            }
        }

        // 加载远端FileInfo
        void LoadFileInfo_Remote(Action<bool> onFinish)
        {
            Action<WWW, ResFileLoader> onLoaded = (www, loader) =>
            {
                //error occurs
                if (www == null || string.IsNullOrEmpty(www.error) == false)
                {
                    LastError = ResErrorCode.DownloadFailed;
                    onFinish(false);
                    return;
                }
                // parse file info
                Debug.Log("LoadFileInfo www.text=" + www.text);
                if (m_RemoteVersionInfo.ParseFileInfo(www.text) == false)
                {
                    LastError = ResErrorCode.CorruptFile;
                    onFinish(false);
                    return;
                }
                onFinish(true);
            };
            LoadFromRemote(Context.FileInfoPath, onLoaded, true);
        }
    }
}
