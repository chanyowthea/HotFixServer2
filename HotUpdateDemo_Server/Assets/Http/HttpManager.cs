using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using TsiU;

namespace GCommon
{
    public interface IHttpManagerImpl
    {
        void OnStartRequest(HttpRequest curHttpReq);
        void OnEndRequest(HttpRequest nextHttpReq);
        void OnUnauthorized();
        void OnInvokeHTTPResult(string url, string cmd, HttpErrorCode errorCode, string errorMsg);
        void OnRetryFailed(HttpRequest curHttpReq);
    }
    public enum EHttpDataType
    {
        Protobuf, Json
    }

    //TJQ: http req/res with timeout
    public class HttpManager : TSingleton<HttpManager>
    {
        public static readonly uint HTTPOPTION_DEFAULT = 0;
        public static readonly uint HTTPOPTION_SILENCE = 1 << 1;

        private const float DEFAULT_TIMEOUT = 10f;
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
        class TokenInfo
        {
            public string Token;
            public uint TTL;
            public TokenInfo()
            {
                Token = string.Empty;
            }
        }
        private Queue<HttpRequest> m_Requests;
        private HttpRequest m_CurRequest;
        private HttpRequest m_TokenRefreshRequest;
        private TimeoutTimer m_TimeoutTimer;
        private TokenInfo m_TokenInfo;
        private IHttpManagerImpl m_Impl;
        private EHttpDataType m_DataType;
        public string LastErrorMessage
        {
            get
            {
                if (m_CurRequest != null)
                {
                    return m_CurRequest.LastErrorMessage;
                }
                return string.Empty;
            }
        }
        public void Init()
        {
            m_Requests = new Queue<HttpRequest>();
            m_TimeoutTimer = new TimeoutTimer();
            m_CurRequest = null;
            m_TokenInfo = new TokenInfo();
            m_TokenRefreshRequest = null;
            m_DataType = EHttpDataType.Protobuf;
        }
        public void SetImpl(IHttpManagerImpl impl)
        {
            m_Impl = impl;
        }
        public void UpdateTokenInfo(string token, uint ttl)
        {
            m_TokenInfo.Token = token;
            m_TokenInfo.TTL = ttl;
        }
        public string GetToken()
        {
            return m_TokenInfo.Token;
        }
        public void Clear()
        {
            if (m_CurRequest != null)
            {
                m_CurRequest.Dispose();
                m_CurRequest = null;
            }
            m_TokenRefreshRequest = null;
            m_Requests.Clear();
        }
        public void Update(float gameTime)
        {
            if (m_CurRequest != null)
            {
                if (m_CurRequest.IsFinished() || m_TimeoutTimer.IsExpired(gameTime))
                {
                    if (m_Impl != null)
                    {
                        m_Impl.OnEndRequest(m_Requests.Count == 0 ? null : m_Requests.Peek());
                    }
                    //check if unauthorized
                    if (m_CurRequest.IsUnauthorized() && m_Impl != null)
                    {
                        if (!m_CurRequest.NeedRetry())
                        {
                            m_Impl.OnRetryFailed(m_CurRequest);
                        }
                        else
                        {
                            m_Impl.OnUnauthorized();
                            //re-send
                            m_Requests.Enqueue(m_CurRequest);
                        }
                    }
                    else
                    {
                        m_CurRequest.Notify(m_Impl);
                    }
                    if (m_CurRequest != null) //re-check
                    {
                        m_CurRequest.Dispose();
                        m_CurRequest = null;
                    }
                }
            }
            if (m_CurRequest == null)
            {
                if (m_TokenRefreshRequest != null)
                {
                    m_CurRequest = m_TokenRefreshRequest;
                    m_TokenRefreshRequest = null;
                }
                else if (m_Requests.Count > 0)
                {
                    m_CurRequest = m_Requests.Dequeue();
                }
                if (m_CurRequest != null)
                {
                    if (m_Impl != null)
                    {
                        m_Impl.OnStartRequest(m_CurRequest);
                    }
                    m_CurRequest.Start(m_TokenInfo.Token);
                    m_TimeoutTimer.Start(gameTime, m_CurRequest.Timeout);
                }
            }
        }
        public void RequestRefreshToken<T>(string url, string cmd, object data, Action<HttpErrorCode, object> onFinished, float timeout = 0)
        {
            if (m_TokenRefreshRequest != null)
            {
                return;
            }
            //gen request
            m_TokenRefreshRequest = CreatePostReq<T>(url, cmd, data, onFinished, timeout);
            if (m_TokenRefreshRequest == null)
            {
                onFinished(HttpErrorCode.InvalidArgs, null);
            }
        }
        public void RequestPost(string url, string cmd, object data) //no response needed
        {
            //gen request
            HttpRequest req = CreatePostReq(url, cmd, data);
            if (req != null)
            {
                m_Requests.Enqueue(req);
            }
        }
        public void RequestPost<T>(string url, string cmd, object data, Action<HttpErrorCode, object> onFinished, float timeout = 0, uint http_option = 0)
        {
            //gen request
            HttpRequest req = CreatePostReq<T>(url, cmd, data, onFinished, timeout, http_option);
            if (req != null)
            {
                m_Requests.Enqueue(req);
            }
            else
            {
                onFinished(HttpErrorCode.InvalidArgs, null);
            }
        }
        public void RequestGet<T>(string url, string cmd, Action<HttpErrorCode, object> onFinished, float timeout, uint http_option = 0, params object[] args)
        {
            if (args.Length % 2 != 0)
            {
                onFinished(HttpErrorCode.InvalidArgs, null);
                return;
            }
            //combine url
            int argLen = args.Length / 2;
            string paramStr = "";
            for (int i = 0; i < argLen; i++)
            {
                if (i == 0)
                {
                    paramStr += "?";
                }
                else
                {
                    paramStr += "&";
                }
                paramStr += (args[i * 2].ToString() + "=" + Uri.EscapeDataString(args[i * 2 + 1].ToString()));
            }
            //gen request
            HttpRequest req = new HttpRequest();
            req.URL = Uri.EscapeUriString(url) + cmd + paramStr;
            req.Cmd = cmd;
            req.OnFinished = onFinished;
            req.DataType = m_DataType;
            req.Data = null;
            req.ResponseType = typeof(T);
            req.HttpOption = http_option;
            if (timeout <= 0)
            {
                timeout = DEFAULT_TIMEOUT;
            }
            req.Timeout = timeout;
            m_Requests.Enqueue(req);
        }
        private HttpRequest CreatePostReq(string url, string cmd, object data)
        {
            try
            {
                HttpRequest req = new HttpRequest();
                req.URL = Uri.EscapeUriString(url + cmd);
                req.Cmd = cmd;
                req.OnFinished = null;
                req.DataType = m_DataType;
                if (data != null)
                {
                    if (m_DataType == EHttpDataType.Protobuf)
                    {
                        MemoryStream stream = new MemoryStream();
                        ProtoBuf.Serializer.Serialize(stream, data);
                        req.Data = stream.ToArray();
                    }
                    else if (m_DataType == EHttpDataType.Json)
                    {
                        req.Data = System.Text.Encoding.UTF8.GetBytes(JsonMapper.ToJson(data));
                    }
                }
                else
                {
                    req.Data = null;
                }
                req.ResponseType = null;
                req.Headers = new Dictionary<string, string>();
                req.Timeout = DEFAULT_TIMEOUT;
                return req;
            }
            catch
            {
            }
            return null;
        }
        private HttpRequest CreatePostReq<T>(string url, string cmd, object data, Action<HttpErrorCode, object> onFinished, float timeout = 0, uint http_option = 0)
        {
            try
            {
                HttpRequest req = new HttpRequest();
                req.URL = Uri.EscapeUriString(url + cmd);
                req.Cmd = cmd;
                req.OnFinished = onFinished;
                req.DataType = m_DataType;
                req.HttpOption = http_option;
                if (data != null)
                {
                    if (m_DataType == EHttpDataType.Protobuf)
                    {
                        MemoryStream stream = new MemoryStream();
                        ProtoBuf.Serializer.Serialize(stream, data);
                        req.Data = stream.ToArray();
                    }
                    else if (m_DataType == EHttpDataType.Json)
                    {
                        req.Data = System.Text.Encoding.UTF8.GetBytes(JsonMapper.ToJson(data));
                    }
                }
                else
                {
                    req.Data = null;
                }
                req.ResponseType = typeof(T);
                req.Headers = new Dictionary<string, string>();
                if (timeout <= 0)
                {
                    timeout = DEFAULT_TIMEOUT;
                }
                req.Timeout = timeout;
                return req;
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message); 
            }
            return null;
        }
    }
}
