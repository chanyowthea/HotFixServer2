using System;
using System.Collections.Generic;
using UnityEngine;

namespace GCommon
{
    public class ResFileLoader
    {
        private const float DEFAULT_TIMEOUT = 5f;
        class TimeoutTimer
        {
            private float m_EndTime;
            public void Start(float gameTime, float timeout)
            {
                m_EndTime = gameTime + timeout;
            }
            public bool IsExpired(float gameTime)
            {
                return gameTime >= m_EndTime;
            }
        }
        public string FullPath;
        public string RelativePath;
        public int RetryCount;
        public WWW HttpWWW;
        public Action<WWW, ResFileLoader> OnLoaded;
        //
        private TimeoutTimer m_Timeout;
        private float m_LastProgress;
        public void Start(float time)
        {
            if (HasStarted())
            {
                return;
            }
            RetryCount++;
            m_Timeout = new TimeoutTimer();
            m_Timeout.Start(time, time + DEFAULT_TIMEOUT);
            m_LastProgress = 0;
            Debug.LogError("ResFileLoader.Start FullPath=" + FullPath); 
            HttpWWW = new WWW(FullPath);
        }
        public bool HasStarted()
        {
            return HttpWWW != null;
        }
        public bool IsFinished()
        {
            return HttpWWW != null && HttpWWW.isDone;
        }
        public bool IsTimeout(float time)
        {
            return m_Timeout != null && m_Timeout.IsExpired(time);
        }
        public void UpdateProgress(float time)
        {
            if (HttpWWW != null && HttpWWW.error == null)
            {
                float curProgress = HttpWWW.progress;
                if (curProgress > m_LastProgress)
                {
                    m_Timeout.Start(time, time + DEFAULT_TIMEOUT);
                    m_LastProgress = curProgress;
                }
            }
        }
        public void Notify()
        {
            if (HttpWWW.isDone == false)
            {
                Debug.LogWarning("load " + FullPath + " timeout");
                OnLoaded(null, this);
            }
            else
            {
                OnLoaded(HttpWWW, this);
            }
        }
        public void Dispose()
        {
            if (HttpWWW != null)
            {
                HttpWWW.Dispose();
                HttpWWW = null;
            }
        }
    }
    public class ResFileLoaderController
    {
        private int m_MaxLoaderCountSimultaneously;
        private LinkedList<ResFileLoader> m_Loaders = new LinkedList<ResFileLoader>();
        private LinkedList<ResFileLoader> m_RunningLoaders = new LinkedList<ResFileLoader>();
        public ResFileLoaderController(int maxLoaderCountSimultaneously)
        {
            m_MaxLoaderCountSimultaneously = maxLoaderCountSimultaneously;
        }
        public void AddLoader(ResFileLoader loader, bool addToFirst = false)
        {
            Debug.Log("load: " + loader.FullPath);
            if (addToFirst)
            {
                m_Loaders.AddFirst(loader);
            }
            else
            {
                m_Loaders.AddLast(loader);
            }
        }
        public void Clear()
        {
            LinkedListNode<ResFileLoader> loaderNode = m_RunningLoaders.First;
            while (loaderNode != null)
            {
                ResFileLoader l = loaderNode.Value;
                l.Dispose();
                loaderNode = loaderNode.Next;
            }
            m_RunningLoaders.Clear();
            m_Loaders.Clear();
        }
        private List<ResFileLoader> m_FinishedLoader = new List<ResFileLoader>();
        public void Update(float time)
        {
            if (m_RunningLoaders.Count > 0)
            {
                LinkedListNode<ResFileLoader> loaderNode = m_RunningLoaders.First;
                while (loaderNode != null)
                {
                    ResFileLoader l = loaderNode.Value;
                    if (l.HasStarted() == false)
                    {
                        l.Start(time);
                    }
                    if (l.IsFinished() || l.IsTimeout(time))
                    {
                        m_FinishedLoader.Add(l);
                        //remove this loader
                        LinkedListNode<ResFileLoader> tmp = loaderNode.Next;
                        m_RunningLoaders.Remove(loaderNode);
                        loaderNode = tmp;
                    }
                    else
                    {
                        l.UpdateProgress(time);
                        //TODO, get detail progress
                        loaderNode = loaderNode.Next;
                    }
                }
                //call notify
                foreach (ResFileLoader l in m_FinishedLoader)
                {
                    l.Notify();
                    l.Dispose();
                }
                m_FinishedLoader.Clear();
            }
            int addCount = 0;
            if (m_MaxLoaderCountSimultaneously <= 0)
            {
                addCount = m_Loaders.Count;
            }
            else if (m_RunningLoaders.Count < m_MaxLoaderCountSimultaneously)
            {
                addCount = m_MaxLoaderCountSimultaneously - m_RunningLoaders.Count;
            }
            if (addCount > 0 && m_Loaders.Count > 0)
            {
                m_RunningLoaders.AddLast(m_Loaders.First.Value);
                m_Loaders.RemoveFirst();
            }
        }
        public bool IsFinished()
        {
            return m_Loaders.Count == 0 && m_RunningLoaders.Count == 0;
        }
    }
}
