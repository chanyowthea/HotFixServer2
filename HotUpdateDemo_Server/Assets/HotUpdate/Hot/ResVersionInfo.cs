using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GCommon;
using System.IO;
using System.Text;

namespace HotFix
{
    public class FileInfo
    {
        // 是从StreamingAssets加载还是从Cache中加载
        public enum OpState
        {
            Local, Cache
        }
        public string FullName;
        public string Hash;
        public long Size;
        public OpState State;

        public virtual string Print()
        {
            return string.Format("{0}, {1}, {2}, {3}", FullName, Hash, Size, State);
        }
        public virtual FileInfo Clone()
        {
            FileInfo ret = new FileInfo();
            ret.FullName = FullName;
            ret.Hash = Hash;
            ret.Size = Size;
            ret.State = State;
            return ret;
        }
    }

    public class ResVersionInfo
    {
        public uint MajorVersion;
        public uint MinorVersion;
        public Dictionary<string, FileInfo> m_FileInfos { get; private set; }

        public virtual void LoadVersionInfo(Action<ResErrorCode> onSetHotUpdater, string cachedVersionFile,
            string removeVersion, Action<bool> onVersionInfoLoaded)
        {
            // TODO
        }

        public string GetVersionString()
        {
            return string.Format("{0}.{1}.{2}",
                    (MajorVersion >> 16) & 0x0000ffff, MajorVersion & 0x0000ffff, MinorVersion);
        }

        public virtual bool Save(string cachedVersionFile, string cachedFileInfoFile)
        {
            try
            {
                //save file info
                string cachedFileInfoDirectory = Path.GetDirectoryName(cachedFileInfoFile);
                if (Directory.Exists(cachedFileInfoDirectory) == false)
                {
                    Directory.CreateDirectory(cachedFileInfoDirectory);
                }
                using (FileStream s = File.Create(cachedFileInfoFile))
                {
                    foreach (KeyValuePair<string, FileInfo> v in m_FileInfos)
                    {
                        string fStr = string.Format("{0},{1},{2},{3}\n", v.Value.FullName, v.Value.Hash, v.Value.Size, (int)v.Value.State);
                        byte[] b = Encoding.UTF8.GetBytes(fStr);
                        s.Write(b, 0, b.Length);
                    }
                }

                //save version info
                string cachedVersionFileDirectory = Path.GetDirectoryName(cachedVersionFile);
                if (Directory.Exists(cachedVersionFileDirectory) == false)
                {
                    Directory.CreateDirectory(cachedVersionFileDirectory);
                }
                string verStr = string.Format("{0}.{1}.{2}",
                    (this.MajorVersion >> 16) & 0x0000ffff, this.MajorVersion & 0x0000ffff, this.MinorVersion);
                using (FileStream s = File.Create(cachedVersionFile))
                {
                    byte[] b = Encoding.UTF8.GetBytes(verStr);
                    s.Write(b, 0, b.Length);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public virtual bool ParseFileInfo(string content)
        {
            m_FileInfos = new Dictionary<string, FileInfo>();
            string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                return true;
            }
            char[] delim = new char[] { ',' };
            foreach (string fileInfoStr in lines)
            {
                string[] items = fileInfoStr.Split(delim, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length != 4)
                {
                    Debug.LogWarning("invald format found, " + fileInfoStr);
                    continue;
                }
                string fullName = items[0];
                if (m_FileInfos.ContainsKey(fullName))
                {
                    Debug.LogWarning("duplicate name, " + fileInfoStr);
                    continue;
                }
                string hash = items[1];
                uint size = uint.Parse(items[2]);
                FileInfo fileInfo = new FileInfo();
                fileInfo.FullName = fullName;
                fileInfo.Hash = hash;
                fileInfo.Size = size;
                fileInfo.State = (FileInfo.OpState)int.Parse(items[3]);
                m_FileInfos.Add(fileInfo.FullName, fileInfo);
            }
            return true;
        }

        public static bool ParseVersion(string content, ref uint majorVersion, ref uint minorVersion)
        {
            string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                return false;
            }
            string versionInfo = lines[0];
            string[] vers = versionInfo.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (vers.Length != 3)
            {
                return false;
            }
            majorVersion = uint.Parse(vers[0]) << 16 | uint.Parse(vers[1]);
            minorVersion = uint.Parse(vers[2]);
            return true;
        }

    }
}
