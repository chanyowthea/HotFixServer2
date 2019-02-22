using GCommon;
using LitJson;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class GameConfig
{
    public static string VerAddr = "http://localhost:8080/";
    static public string ServerAddr;
    static public bool SkipVersionCheck = false;
    static public bool SkipResourceDownload = false;
}

//    public enum EAppStore
//    {
//        GooglePlay, Apple, Official
//    }
//    //server addr
//    static public string VerAddr;

//    /// <summary>
//    /// 内容分发网络（Content Distribution Network）
//    /// </summary>
//    static public string CDNAddr;
//    static public string ServerAddr;
//    static public string LogServerAddr;
//    static public bool IsIOSReview;

//    //version check related
//    static public bool SkipVersionCheck = false;
//    static public bool SkipResourceDownload = false;

//    // temp debug
//    static public bool LegacyGuest = false;

//    static public EAppStore AppStore = EAppStore.Official;
//    static public string AppStoreAddr = "http://www.freefiremobile.com/";

//    //Game Setting
//    static public bool UseAssetBundle;

//    //Camera Settings
//    static public float CAM_OFFSET_FOR_SNIPER = 0;

//    //Debug only
//    static public bool MuteSound;
//    static public string DevId;
//    static public bool DebugMode;
//    static public bool ShowDebugInfo;
//    static public bool DrawAimAssistDebugInfo;
//    static public bool ChooseRegionEnabled;
//    static public bool TestModeEnabled;
//    static public bool NoFBLogin;

//    // Garena SDK config

//    static public bool garenaProduction =
//#if SBT_BUILD
//            false
//#elif RCT_BUILD
//            false
//#elif BETA_BUILD
//            true
//#else
//            true
//#endif
//        ;
//    static public string garenaAppId = "100067";
//    static public string garenaAppKeySandbox = "8a9449649a1748f16cc0b922eee9c83cea4f3ac6f5981211536ad9fa0db94133";
//    static public string garenaAppKeyProduction = "2ee44819e9b4598845141067b281621874d0d5d7af9d8f7e00c1e54715b7d1e3";
//    static public string garenaPushAppkeySandbox = "";
//    static public string garenaPushAppkeyProduction = "";


//    class LocalConfig
//    {
//        //server setting override
//        public string serverUrl;
//        public string CDNAddr;
//        public string region;
//        public string verAddr;
//        public bool skipVersionCheck;
//        public bool skipResourceDownload;

//        //debug test
//        public string BillboardDesc;
//        public bool AnnouncementDebug;
//        public bool garenaOverride;
//        public bool garenaSandbox;
//        public bool legacyGuest;

//        public bool rebateCardOverride;
//        public bool rebateCardEnabled;

//        public bool resetGuest;

//        public string testShareUrl;
//        public string testInviteUrl;

//        public string testPayMainUrl_Paid;
//        public string testPayMainUrl_Unpaid;
//        public string testPayItemUrl_iOS;
//        public string testPayIAPButtonUrl_Android;
//        public string testPayAdImageUrl_Android;
//        public string testPayAdClickUrl_Android;

//        public bool logoutStopXG;
//    }

//    static LocalConfig m_LocalConfig;

//    public static string TestPayMainUrl_Paid
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testPayMainUrl_Paid;
//            }
//            return string.Empty;
//        }
//    }

//    public static string TestPayMainUrl_Unpaid
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testPayMainUrl_Unpaid;
//            }
//            return string.Empty;
//        }
//    }
//    public static string TestPayItemUrl_iOS
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testPayItemUrl_iOS;
//            }
//            return string.Empty;
//        }
//    }
//    public static string TestPayIAPButtonUrl_Android
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testPayIAPButtonUrl_Android;
//            }
//            return string.Empty;
//        }
//    }
//    public static string TestPayAdImageUrl_Android
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testPayAdImageUrl_Android;
//            }
//            return string.Empty;
//        }
//    }
//    public static string TestPayAdClickUrl_Android
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testPayAdClickUrl_Android;
//            }
//            return string.Empty;
//        }
//    }


//    public static string TestShareUrl
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testShareUrl;
//            }
//            return string.Empty;
//        }
//    }

//    public static string TestInviteUrl
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.testInviteUrl;
//            }
//            return string.Empty;
//        }
//    }

//    static public string BillboardTestDesc
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.BillboardDesc;
//            }
//            return string.Empty;
//        }
//    }

//    static public bool ResetGuest
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.resetGuest;
//            }
//            return false;
//        }
//    }

//    static public bool RebateCardOverride
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.rebateCardOverride;
//            }
//            return false;
//        }
//    }

//    static public bool RebateCardEnabled
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.rebateCardEnabled;
//            }
//            return true;
//        }
//    }

//    static public bool LogoutStopXG
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.logoutStopXG;
//            }
//            return false;
//        }
//    }

//    static public bool IsAnnouncementDebugMode
//    {
//        get
//        {
//            if (m_LocalConfig != null)
//            {
//                return m_LocalConfig.AnnouncementDebug;
//            }
//            return false;
//        }
//        set
//        {
//            if (m_LocalConfig != null)
//            {
//                m_LocalConfig.AnnouncementDebug = value;
//            }
//        }
//    }

//    static public void UpdateServerConfigFromHTTPVerGet(string serverAddr, string cdnAddr, bool isIOSReview)
//    {
//        ServerAddr = serverAddr;
//        CDNAddr = cdnAddr;
//        IsIOSReview = isIOSReview;

//        //#if UNITY_EDITOR
//        //            //override in game startup for easy test
//        //            ServerAddr = GameStartup.instance.GatewayAddr;
//        //            CDNAddr = GameStartup.instance.CDNAddr;
//        //#endif
//        if (m_LocalConfig != null)
//        {
//            if (!string.IsNullOrEmpty(m_LocalConfig.serverUrl))
//            {
//                ServerAddr = m_LocalConfig.serverUrl;
//            }
//            if (!string.IsNullOrEmpty(m_LocalConfig.CDNAddr))
//            {
//                CDNAddr = m_LocalConfig.CDNAddr;
//            }
//        }
//    }

//    static LocalConfig ReadUrlFromLocalConfig()
//    {
//        string overrideFileName = "localConfig.json";
//#if UNITY_EDITOR
//        string localConfigPath = Path.Combine(Application.dataPath, overrideFileName);
//#else
//            string localConfigPath = Path.Combine(Application.persistentDataPath, overrideFileName);
//#endif
//        Debug.Log("Load Local Config File From " + localConfigPath);
//        if (System.IO.File.Exists(localConfigPath) == true)
//        {
//            //object config_object = JsonMapper.ToObject(File.ReadAllText(localConfigPath), typeof(LocalConfig));
//            //if (config_object != null)
//            //{
//            //    Debugger.Log("Load Local Config Success");
//            //    local_config = (LocalConfig)config_object;
//            //}
//            //else
//            //{
//            //    Debugger.LogWarning("Load Local Config  Failed");
//            //}
//            try
//            {
//                LocalConfig local_config = new LocalConfig();
//                string localConfigString = File.ReadAllText(localConfigPath);
//                local_config = JsonUtility.FromJson<LocalConfig>(localConfigString);
//                Debug.Log("Load Local Config: " + localConfigString);
//                return local_config;
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogWarning("Load Local Config Failed: " + e.ToString());
//            }
//        }
//        else
//        {
//            Debug.LogWarning("Couldn't Find Local Config File");
//        }
//        return null;
//    }

//    static public void Reset()
//    {
//#if RELEASE_BUILD
//#if NO_AB
//            UseAssetBundle = false;
//#else
//            UseAssetBundle = true;
//#endif
//            MuteSound = false;
//            DevId = "";
//            DebugMode = false;
//            ShowDebugInfo = false;
//            DrawAimAssistDebugInfo = false;
//            ChooseRegionEnabled = false;
//            TestModeEnabled = false;
//#else
//        DebugMode = true;
//        ChooseRegionEnabled = true;
//        TestModeEnabled = true;
//#endif

//#if LIVE_BUILD
//            ShowDebugInfo = false;
//            ChooseRegionEnabled = false;
//            TestModeEnabled = false;
//#else
//        ShowDebugInfo = true;
//        ChooseRegionEnabled = true;
//        TestModeEnabled = true;
//#endif

//#if BETA_BUILD
//            ShowDebugInfo = false;
//#endif
//        //NoFBLogin = true; //TMP, remove in cb

//#if !UNITY_EDITOR
//#endif

//#if UNITY_ANDROID
//#if GOOGLEPLAY_STORE
//            AppStore = EAppStore.GooglePlay;
//            AppStoreAddr = "http://play.google.com/store/apps/details?id=com.dts.freefireth";
//#else
//            AppStore = EAppStore.Official;
//            AppStoreAddr = "http://www.freefiremobile.com/";
//#endif
//#elif UNITY_IPHONE
//            AppStore = EAppStore.Apple;
//            AppStoreAddr = "https://itunes.apple.com/app/id1300146617";
//#else
//        AppStore = EAppStore.Official;
//        AppStoreAddr = "http://www.freefiremobile.com/";
//#endif

//#if LIVE_BUILD
//#if USING_EXP_SERVER //for experience
//            VerAddr = "http://119.28.113.79:8080/exp/";
//#else
//            VerAddr = "https://version.common.freefiremobile.com/live/";
//#endif
//#elif BETA_BUILD
//            VerAddr = "https://version.common.freefiremobile.com/beta/";
//#elif RCT_BUILD
//            VerAddr = "http://10.21.100.151:8080/rct/";
//#else
//        VerAddr = "http://localhost:8080/"; //SBT_BUILD
//#endif

//#if UNITY_EDITOR
//        //override in game startup for easy test
//        //VerAddr = GameStartup.instance.VerAddr;
//#endif
//        m_LocalConfig = ReadUrlFromLocalConfig();
//        //append platform info
//        //VerAddr = VerAddr + ResHotUpdater.instance.PlatformIdentifier + "/";

//        if (m_LocalConfig != null && !string.IsNullOrEmpty(m_LocalConfig.verAddr))
//        {
//            VerAddr = m_LocalConfig.verAddr;
//        }
//        Debug.LogError("VerAddr=" + VerAddr); 

//#if UNITY_EDITOR
//        //override in game startup for easy test
//        SkipVersionCheck = false;
//#endif


//#if BETA_BUILD
//            SkipVersionCheck = true;
//#endif
//        if (m_LocalConfig != null)
//        {
//            //load debug config
//            SkipResourceDownload = m_LocalConfig.skipResourceDownload;
//            SkipVersionCheck = m_LocalConfig.skipVersionCheck;
//            if (m_LocalConfig.garenaOverride)
//            {
//                garenaProduction = !m_LocalConfig.garenaSandbox;
//            }
//#if !LIVE_BUILD
//            LegacyGuest = m_LocalConfig.legacyGuest;
//#endif
//        }

//    }
//}