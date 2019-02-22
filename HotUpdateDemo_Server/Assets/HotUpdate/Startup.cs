using GCommon;
using HotFix;
using ProtoBuf;
using System;
using System.Collections;
using UnityEngine;

// 打包之后Assets/StreamingAssets存放AssetBundle资源
// 打包之后StreamingAssets只能读不能写
// 热更新之前,检查StreamingAssets目录中的文件,再比较缓存中的文件
// 如果缓存没有或者版本一致,那么比较网络文件,如果网络文件版本高,那么下载网络文件到缓存
// 如果缓存中版本高,那么比较网络文件,如果网络文件版本与缓存版本一致,则使用缓存文件
// 缓存中存储打包后更新的文件,并不拥有所有AssetBundle

public class Startup : MonoBehaviour
{
    public Action OnLaunchGame;
    public bool IsRestart;

    private float m_Progress;
    private bool m_IsHotUpdating;
    private int m_LastSleepTimeout = SleepTimeout.SystemSetting;
    private bool m_LastRunInBackground = false;
    private bool m_Cleanup = false;

    void Start()
    {
        HttpManager.instance.Init();
        //GameConfig.Reset(); 

        m_Progress = 10;
        m_IsHotUpdating = false;
        OnUIOpen();
    }

    void OnUIOpen()
    {
        m_LastSleepTimeout = Screen.sleepTimeout;
        m_LastRunInBackground = Application.runInBackground;
        CheckVersion();
    }

    void Destroy()
    {
        StopAllCoroutines();
        //clear http manager
        HttpManager.instance.Clear();
    }
    void Update()
    {
        HttpManager.instance.Update(Time.time);
        ResHotUpdater.instance.Update(Time.time);
        if (m_IsHotUpdating)
        {
            long totalBytes = ResHotUpdater.instance.GetTotalSizeInByte();
            long totalLoadedBytes = ResHotUpdater.instance.GetTotalLoadedSizeInByte();
            if (totalBytes == 0)
            {
                m_Progress = 100;
            }
            else
            {
                m_Progress = Mathf.Clamp((float)totalLoadedBytes / totalBytes * 100, 10, 100);
            }
            if (MathUnity.IsEqual(m_Progress, 100))
            {

            }
            UpdateProgressInfo();
        }
        if (m_Cleanup)
        {
            ResHotUpdater.instance.Clear();
            m_Cleanup = false;
        }
    }
    enum EHttpVerInfoErrorCode
    {
        OK, ERROR, NOVERSIONFOUND
    }
    [ProtoContract]
    class HttpVerInfo
    {
        [ProtoMember(1)]
        public uint code;
        [ProtoMember(2)]
        public bool is_server_open;
        [ProtoMember(3)]
        public string billboard_msg;
        [ProtoMember(4)]
        public string remote_version;

        // CDN的全称是Content Delivery Network，即内容分发网络。其基本思路是尽可能避开互联网上有可能影响数据传输速度和稳定性的瓶颈和环节，
        // 使内容传输的更快、更稳定。通过在网络各处放置节点服务器所构成的在现有的互联网基础之上的一层智能虚拟网络，
        // CDN系统能够实时地根据网络流量和各节点的连接、负载状况以及到用户的距离和响应时间等综合信息将用户的请求重新导向离用户最近的服务节点上。
        // 其目的是使用户可就近取得所需内容，解决 Internet网络拥挤的状况，提高用户访问网站的响应速度。
        [ProtoMember(5)]
        public string cdn_url;
        [ProtoMember(6)]
        public string server_url;
        [ProtoMember(7)]
        public bool is_review_server;
        [ProtoMember(8)]
        public string appstore_url;
        [ProtoMember(9)]
        public bool force_to_restart_app;
    }
    private bool m_CanInGameHotupdate;

    private void RetrieveVerInfo()
    {
        string lanName = "en";
        //if (GameSettingData.GetLoclizationLanguage() != LocLang.None)
        //{
        //    lanName = LocLangConvert.GetAbbr(GameSettingData.GetLoclizationLanguage());
        //}
        HttpManager.instance.RequestGet<HttpVerInfo>(GameConfig.VerAddr, "ver.php?v=1", (errorCode, res) =>
        {
            if (errorCode == HttpErrorCode.OK)
            {
                HttpVerInfo remoteVerInfo = (HttpVerInfo)res;
                if (remoteVerInfo.is_server_open == false)
                {
                    //ShowMessageBox(remoteVerInfo.billboard_msg, null);
                }
                else
                {
                    //GameConfig.AppStoreAddr = remoteVerInfo.appstore_url; //override
                    if (remoteVerInfo.code == (uint)EHttpVerInfoErrorCode.NOVERSIONFOUND)
                    {
                        OnColdUpdate();
                    }
                    else
                    {
                        //TJQ: guard
                        m_CanInGameHotupdate = (IsRestart == false || (remoteVerInfo.force_to_restart_app == false));

                        //start hotupdate
                        // 在这里就返回了版本信息
                        ResHotUpdater.instance.StartVersionCheck(GameConfig.VerAddr, remoteVerInfo.remote_version);
                    }
                }
            }
            else
            {
                ResHotUpdater.instance.LastError = ResErrorCode.DownloadFailed;
                OnHotUpdateError();
            }
        }, 30, 0,
        "version", ResHotUpdater.instance.Version,
        "lang", lanName,
        "device", Context.PlatformIdentifier,
        "appstore", "apple");
    }
    private void CheckVersion()
    {
        //init value
        m_CanInGameHotupdate = true;
        Context.NoResourceDownload = GameConfig.SkipResourceDownload;
        Context.CoroutineHolder = this;
        Context.OnGetLocalVerionFinished = (result) =>
        {
            Debug.Log("OnGetLocalVerionFinished: " + result);
            switch (result)
            {
                case ResHotUpdateResult.ERROR:
                    OnHotUpdateError();
                    break;
                case ResHotUpdateResult.PASS:
                    if (GameConfig.SkipVersionCheck)
                    {
                        ResHotUpdater.instance.StartGetLocalFileInfo();
                    }
                    else
                    {
                        RetrieveVerInfo(); //get version info from remote http server
                    }
                    break;
            }
        };
        Context.OnStarted = (result) =>
        {
            Debug.Log("onResHotUpdateStarted: " + result);
            switch (result)
            {
                case ResHotUpdateResult.HOTUPDATE:
                    //TJQ: it's a guard in case that in-game hotupdate is not supported
                    if (m_CanInGameHotupdate)
                    {
                        //start hot update
                        long totalBytes = ResHotUpdater.instance.GetTotalSizeInByte();
                        float toMB = (float)((double)totalBytes / (1024 * 1024));
                        StartCoroutine(StartHotUpdating());
                    }
                    else
                    {
                        //ask player to restart app
                        Action okAction = null;
#if !UNITY_IPHONE
                        okAction = () =>
                    {
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#else
                                Application.Quit();
#endif
                        };
#endif
                    }
                    break;
                case ResHotUpdateResult.PASS:
                    OnHotUpdateFinished(true);
                    break;
                case ResHotUpdateResult.COLDUPDATE:
                    OnColdUpdate();
                    break;
                case ResHotUpdateResult.ERROR:
                    OnHotUpdateError();
                    break;
            }
        };
        OnStartUpdating();
        //start hot updater
        ResHotUpdater.instance.Clear();
        ResHotUpdater.instance.Init();
        //start local version
        ResHotUpdater.instance.StartGetLocalVersion();
    }
    private void UpdateProgressInfo()
    {

    }
    private void OnColdUpdate()
    {
        OnFinishUpdating();
        //LocManager.instance.DoLoc("TXT_LAUNCHER_MSG_COLDUPDATE"),
        //() =>
        //{
        //    Application.OpenURL(GameConfig.AppStoreAddr); //TODO
        //    }
    }
    private void OnHotUpdateError()
    {
        OnFinishUpdating();

        string id = "TXT_LAUNCHER_ERR_UNKNOWNREASON";
        switch (ResHotUpdater.instance.LastError)
        {
            case ResErrorCode.CorruptFile:
                id = "TXT_LAUNCHER_ERR_CURRUPTFILE";
                break;
            case ResErrorCode.DownloadFailed:
                id = "TXT_LAUNCHER_ERR_DOWNLOADFAILED";
                break;
            case ResErrorCode.SaveFailed:
                id = "TXT_LAUNCHER_ERR_SAVEFAILED";
                break;
        }

        //StartCoroutine(StartCheckVersion());
    }
    private void OnHotUpdateFinished(bool result)
    {
        m_IsHotUpdating = false;
        OnFinishUpdating();
        if (result == true)
        {
            m_Progress = 100f;
            float value = m_Progress / 100f;
            Debug.Log("progress=" + value);
            UpdateProgressInfo();

            StartCoroutine(StartLaunchGame());
        }
        else
        {
            OnHotUpdateError();
        }
    }
    IEnumerator StartLaunchGame()
    {
        yield return new WaitForSeconds(0.5f);
        Action caller = OnLaunchGame;
        if (caller != null)
        {
            OnLaunchGame = null;
            caller.Invoke();
        }
    }
    IEnumerator StartHotUpdating()
    {
        yield return new WaitForSeconds(0.5f);
        m_IsHotUpdating = true;
        ResHotUpdater.instance.StartHotUpdateDownload(OnHotUpdateFinished);
    }
    IEnumerator StartCheckVersion()
    {
        yield return new WaitForSeconds(0.5f);
        CheckVersion();
    }
    private void OnStartUpdating()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;
    }
    private void OnFinishUpdating()
    {
        Screen.sleepTimeout = m_LastSleepTimeout;
        Application.runInBackground = m_LastRunInBackground;
    }
}
