//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnityEngine;

//namespace GCommon
//{
//    public class ResVersionInfo
//    {
//        public uint MajorVersion;
//        public uint MinorVersion;
//        //load from remote or local
//        private bool m_IsRemote;
//        private bool m_IsUsingEmbed;
//        //file info
//        public class FileInfo
//        {
//            // 是从StreamingAssets加载还是从Cache中加载
//            public enum OpState
//            {
//                Local, Cache
//            }
//            public string FullName;
//            public string Hash;
//            public long Size;
//            public OpState State;
//            //public string CDNExt;

//            public string Print()
//            {
//                return string.Format("{0}, {1}, {2}, {3}", FullName, Hash, Size, State);
//            }
//            public FileInfo Clone()
//            {
//                FileInfo ret = new FileInfo();
//                ret.FullName = FullName;
//                ret.Hash = Hash;
//                ret.Size = Size;
//                ret.State = State;
//                return ret;
//            }
//        }
//        private Dictionary<string, FileInfo> m_FileInfos;
//        public ResVersionInfo(bool isRemote)
//        {
//            m_IsRemote = isRemote;
//        }
//        public void LoadFromString(ResHotUpdater updaterInst, string removeVersion, Action<bool> onVersionInfoLoaded)
//        {
//            //parse version info
//            uint embedMajorVersion = 0, // 1.0.0 前面两个组成的数字，前面16位是1后面16位是0
//                embedMinorVersion = 0; // 最后一个0
//            if (ParseVersion(removeVersion, ref embedMajorVersion, ref embedMinorVersion) == false)
//            {
//                updaterInst.LastError = ResErrorCode.CorruptFile;
//                onVersionInfoLoaded(false);
//                return; 
//            }
//            this.MajorVersion = embedMajorVersion;
//            this.MinorVersion = embedMinorVersion;
//            m_IsUsingEmbed = true;
//            if (m_IsRemote == false)
//            {
//                //load cached version file
//                try
//                {
//                    ResHotUpdaterContext context = updaterInst.Context;
//                    string cachedVersionFile = Path.Combine(context.CacheAddr, context.VersionInfoPath);
//                    string cachedVersionFileDirectory = Path.GetDirectoryName(cachedVersionFile);
//                    if (Directory.Exists(cachedVersionFileDirectory) == false)
//                    {
//                        Directory.CreateDirectory(cachedVersionFileDirectory);
//                    }
//                    if (File.Exists(cachedVersionFile))
//                    {
//                        string cachedContent = null;
//                        using (FileStream s = File.OpenRead(cachedVersionFile))
//                        {
//                            byte[] bs = new byte[s.Length];
//                            s.Read(bs, 0, bs.Length);
//                            cachedContent = Encoding.UTF8.GetString(bs, 0, bs.Length);
//                        }
//                        Debug.Log("read cache: " + cachedVersionFile);
//                        uint cachedMajorVersion = 0, cachedMinorVersion = 0;
//                        if (ParseVersion(cachedContent, ref cachedMajorVersion, ref cachedMinorVersion) == false)
//                        {
//                            updaterInst.LastError = ResErrorCode.CorruptFile;
//                            onVersionInfoLoaded(false);
//                            return;
//                        }
//                        //compare embed version file and local file
//                        if (cachedMajorVersion > embedMajorVersion ||
//                            cachedMajorVersion == embedMajorVersion && cachedMinorVersion > embedMinorVersion)
//                        {
//                            m_IsUsingEmbed = false;
//                            this.MajorVersion = cachedMajorVersion;
//                            this.MinorVersion = cachedMinorVersion;
//                        }
//                    }
//                }
//                catch
//                { } //if no file existed
//            }
//            onVersionInfoLoaded(true);
//        }
//        public void Load(ResHotUpdater updaterInst, string versionInfoPath, Action<bool> onVersionInfoLoaded)
//        {
//            Debug.Log("versionInfoPath=" + versionInfoPath);
//            Action<WWW, ResFileLoader> onLoaded = (www, loader) =>
//            {
//                //error occurs
//                if (www == null || string.IsNullOrEmpty(www.error) == false)
//                {
//                    updaterInst.LastError = ResErrorCode.DownloadFailed;
//                    onVersionInfoLoaded(false);
//                    return;
//                }
//                LoadFromString(updaterInst, www.text, onVersionInfoLoaded);
//            };
//            if (m_IsRemote)
//            {
//                string nowTime = ((long)(DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds)).ToString();
//                versionInfoPath += ("?" + nowTime); // TODO
//            }
//            updaterInst.Load(versionInfoPath, onLoaded, m_IsRemote, false);
//        }
//        public void LoadFileInfo(ResHotUpdater updaterInst, string fileInfoPath, Action<bool> onFileInfoLoaded)
//        {
//            Action<WWW, ResFileLoader> onLoaded = (www, loader) =>
//            {
//                //error occurs
//                if (www == null || string.IsNullOrEmpty(www.error) == false)
//                {
//                    updaterInst.LastError = ResErrorCode.DownloadFailed;
//                    onFileInfoLoaded(false);
//                    return;
//                }
//                // parse file info
//                Debug.Log("LoadFileInfo www.text=" + www.text); 
//                if (ParseFile(www.text) == false)
//                {
//                    updaterInst.LastError = ResErrorCode.CorruptFile;
//                    onFileInfoLoaded(false);
//                    return;
//                }
//                onFileInfoLoaded(true);
//            };
//            if (m_IsUsingEmbed)
//            {
//                // 既然是使用本地的,为什么会有Remote的判断呢?
//                //if (m_IsRemote)
//                //{
//                //    string nowTime = ((long)(DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds)).ToString();
//                //    fileInfoPath += ("?" + nowTime);
//                //}
//                updaterInst.Load(fileInfoPath, onLoaded, m_IsRemote);
//            }
//            else
//            {
//                try
//                {
//                    ResHotUpdaterContext context = updaterInst.Context;
//                    string cachedFileInfoFile = Path.Combine(context.CacheAddr, context.FileInfoPath);
//                    if (File.Exists(cachedFileInfoFile))
//                    {
//                        string cachedContent = null;
//                        using (FileStream s = File.OpenRead(cachedFileInfoFile))
//                        {
//                            byte[] bs = new byte[s.Length];
//                            s.Read(bs, 0, bs.Length);
//                            cachedContent = Encoding.UTF8.GetString(bs, 0, bs.Length);
//                        }
//                        Debug.Log("read cache: " + cachedFileInfoFile);
//                        //parse file info
//                        if (ParseFile(cachedContent) == false)
//                        {
//                            updaterInst.Load(fileInfoPath, onLoaded, m_IsRemote); //try embeded file info
//                        }
//                        else
//                        {
//                            onFileInfoLoaded(true);
//                        }
//                        return;
//                    }
//                }
//                catch
//                { }
//                updaterInst.Load(fileInfoPath, onLoaded, m_IsRemote); //try embeded file info
//            }
//        }
//        public string GetVersionString()
//        {
//            return string.Format("{0}.{1}.{2}",
//                    (MajorVersion >> 16) & 0x0000ffff, MajorVersion & 0x0000ffff, MinorVersion);
//        }
//        public Dictionary<string, FileInfo> GetFileInfo()
//        {
//            return m_FileInfos;
//        }
//        public bool Save(ResHotUpdater updaterInst)
//        {
//            try
//            {
//                ResHotUpdaterContext context = updaterInst.Context;
//                //save file info
//                string cachedFileInfoFile = Path.Combine(context.CacheAddr, context.FileInfoPath);
//                string cachedFileInfoDirectory = Path.GetDirectoryName(cachedFileInfoFile);
//                if (Directory.Exists(cachedFileInfoDirectory) == false)
//                {
//                    Directory.CreateDirectory(cachedFileInfoDirectory);
//                }
//                using (FileStream s = File.Create(cachedFileInfoFile))
//                {
//                    foreach (KeyValuePair<string, FileInfo> v in m_FileInfos)
//                    {
//                        string fStr = string.Format("{0},{1},{2},{3}\n", v.Value.FullName, v.Value.Hash, v.Value.Size, (int)v.Value.State);
//                        byte[] b = Encoding.UTF8.GetBytes(fStr);
//                        s.Write(b, 0, b.Length);
//                    }
//                }
//                //save version info
//                string cachedVersionFile = Path.Combine(context.CacheAddr, context.VersionInfoPath);
//                string cachedVersionFileDirectory = Path.GetDirectoryName(cachedVersionFile);
//                if (Directory.Exists(cachedVersionFileDirectory) == false)
//                {
//                    Directory.CreateDirectory(cachedVersionFileDirectory);
//                }
//                string verStr = string.Format("{0}.{1}.{2}",
//                    (this.MajorVersion >> 16) & 0x0000ffff, this.MajorVersion & 0x0000ffff, this.MinorVersion);
//                using (FileStream s = File.Create(cachedVersionFile))
//                {
//                    byte[] b = Encoding.UTF8.GetBytes(verStr);
//                    s.Write(b, 0, b.Length);
//                }
//            }
//            catch
//            {
//                return false;
//            }
//            return true;
//        }
//        private bool ParseVersion(string content, ref uint majorVersion, ref uint minorVersion)
//        {
//            string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
//            if (lines.Length == 0)
//            {
//                return false;
//            }
//            string versionInfo = lines[0];
//            string[] vers = versionInfo.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
//            if (vers.Length != 3)
//            {
//                return false;
//            }
//            majorVersion = uint.Parse(vers[0]) << 16 | uint.Parse(vers[1]);
//            minorVersion = uint.Parse(vers[2]);
//            return true;
//        }
//        private bool ParseFile(string content)
//        {
//            m_FileInfos = new Dictionary<string, FileInfo>();
//            string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
//            if (lines.Length == 0)
//            {
//                return true;
//            }
//            char[] delim = new char[] { ',' };
//            foreach (string fileInfoStr in lines)
//            {
//                string[] items = fileInfoStr.Split(delim, StringSplitOptions.RemoveEmptyEntries);
//                if (items.Length != 4)
//                {
//                    Debug.LogWarning("invald format found, " + fileInfoStr);
//                    continue;
//                }
//                string fullName = items[0];
//                if (m_FileInfos.ContainsKey(fullName))
//                {
//                    Debug.LogWarning("duplicate name, " + fileInfoStr);
//                    continue;
//                }
//                string hash = items[1];
//                uint size = uint.Parse(items[2]);
//                FileInfo fileInfo = new FileInfo();
//                fileInfo.FullName = fullName;
//                fileInfo.Hash = hash;
//                fileInfo.Size = size;
//                fileInfo.State = (FileInfo.OpState)int.Parse(items[3]);
//                m_FileInfos.Add(fileInfo.FullName, fileInfo);
//            }
//            return true;
//        }
//    }
//}
