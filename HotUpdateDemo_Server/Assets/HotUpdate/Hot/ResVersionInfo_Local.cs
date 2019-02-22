using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GCommon;
using UnityEngine;

namespace HotFix
{
    public class ResVersionInfo_Local : ResVersionInfo
    {
        //public bool m_IsUsingEmbed;

        public override void LoadVersionInfo(Action<ResErrorCode> onSetHotUpdater, string cachedVersionFile, string removeVersion, Action<bool> onVersionInfoLoaded)
        {
            //parse version info
            //uint embedMajorVersion = 0, // 1.0.0 前面两个组成的数字，前面16位是1后面16位是0
            //    embedMinorVersion = 0; // 最后一个0
            //if (ParseVersion(removeVersion, ref embedMajorVersion, ref embedMinorVersion) == false)
            //{
            //    if (onSetHotUpdater != null)
            //    {
            //        onSetHotUpdater(ResErrorCode.CorruptFile);
            //    }
            //    onVersionInfoLoaded(false);
            //    return;
            //}
            //this.MajorVersion = embedMajorVersion;
            //this.MinorVersion = embedMinorVersion;
            //m_IsUsingEmbed = true;
            //load cached version file
            try
            {
                string cachedVersionFileDirectory = Path.GetDirectoryName(cachedVersionFile);
                if (Directory.Exists(cachedVersionFileDirectory) == false)
                {
                    Directory.CreateDirectory(cachedVersionFileDirectory);
                }
                if (File.Exists(cachedVersionFile))
                {
                    string cachedContent = null;
                    using (FileStream s = File.OpenRead(cachedVersionFile))
                    {
                        byte[] bs = new byte[s.Length];
                        s.Read(bs, 0, bs.Length);
                        cachedContent = Encoding.UTF8.GetString(bs, 0, bs.Length);
                    }
                    Debug.Log("read cache: " + cachedVersionFile);
                    uint cachedMajorVersion = 0, cachedMinorVersion = 0;
                    if (ParseVersion(cachedContent, ref cachedMajorVersion, ref cachedMinorVersion) == false)
                    {
                        if (onSetHotUpdater != null)
                        {
                            onSetHotUpdater(ResErrorCode.CorruptFile);
                        }
                        onVersionInfoLoaded(false);
                        return;
                    }
                    ////compare embed version file and local file
                    //if (cachedMajorVersion > embedMajorVersion ||
                    //    cachedMajorVersion == embedMajorVersion && cachedMinorVersion > embedMinorVersion)
                    //{
                        //m_IsUsingEmbed = false;
                        this.MajorVersion = cachedMajorVersion;
                        this.MinorVersion = cachedMinorVersion;
                    //}
                }
            }
            catch
            { } //if no file existed
            onVersionInfoLoaded(true);
        }
    }
}
