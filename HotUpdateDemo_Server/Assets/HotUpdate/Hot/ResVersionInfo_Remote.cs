using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GCommon;
using UnityEngine;

namespace HotFix
{
    public class ResVersionInfo_Remote : ResVersionInfo
    {
        public override void LoadVersionInfo(Action<ResErrorCode> onSetHotUpdater, string cachedVersionFile, string removeVersion, Action<bool> onVersionInfoLoaded)
        {
            //parse version info
            uint embedMajorVersion = 0, // 1.0.0 前面两个组成的数字，前面16位是1后面16位是0
                embedMinorVersion = 0; // 最后一个0
            if (ParseVersion(removeVersion, ref embedMajorVersion, ref embedMinorVersion) == false)
            {
                if (onSetHotUpdater != null)
                {
                    onSetHotUpdater(ResErrorCode.CorruptFile);
                }
                onVersionInfoLoaded(false);
                return;
            }
            this.MajorVersion = embedMajorVersion;
            this.MinorVersion = embedMinorVersion;
            onVersionInfoLoaded(true);
        }
    }
}