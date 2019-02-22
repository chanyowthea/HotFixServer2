using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

namespace GCommon
{
    public class HttpRequest
    {
        //-------------------------------------------------------
        private const int MAX_RETRY_COUNT = 5;
        //-------------------------------------------------------
        public string URL;
        public string Cmd;
        public float Timeout;
        public byte[] Data;
        public Dictionary<string, string> Headers;
        public WWW www;
        public Action<HttpErrorCode, object> OnFinished;
        public Type ResponseType;
        public EHttpDataType DataType;
        public uint HttpOption;
        //-------------------------------------------------------
        private int m_SendCount = 0;
        //-------------------------------------------------------
        private string m_LastErrorMessage;
        public string LastErrorMessage
        {
            get
            {
                return m_LastErrorMessage;
            }
        }
        public void Start(string token)
        {
            m_SendCount++;
            m_LastErrorMessage = string.Empty;
            //set token before request sent
            if (Data == null)
            {
                Debug.Log("GET!!! URL=" + URL);
                www = new WWW(URL); //GET
            }
            else
            {
                Headers["Authorization"] = "Bearer " + token;
                www = new WWW(URL, Data, Headers); //POST
                Debug.Log("POST!!! URL=" + URL); 
            }
        }

        public bool NeedRetry()
        {
            return m_SendCount <= MAX_RETRY_COUNT;
        }

        public bool HasRetried()
        {
            return m_SendCount > 1;
        }

        public bool IsFinished()
        {
            if (www != null)
            {
                //Debug.Log("www.isDone=" + www.isDone);
            }
            return www != null && www.isDone;
        }
        public bool IsUnauthorized()
        {
            if (www != null && www.isDone == true && string.IsNullOrEmpty(www.error) == false)
            {
                HttpErrorCode errorCode = GetResponseCode();
                if ((int)errorCode == 401)
                {
                    return true;
                }
            }
            return false;
        }
        public void Notify(IHttpManagerImpl httpImpl)
        {
            if (OnFinished == null)
            {
                return;
            }
            if (www != null && www.isDone == false)
            {
                InvokeFinished(httpImpl, HttpErrorCode.Timeout, null, string.Empty);
                return;
            }
            if (www == null || (www.isDone == true && string.IsNullOrEmpty(www.error) == false))
            {
                InvokeFinished(httpImpl, GetResponseCode(), null, (www == null || www.bytes.Length == 0) ? string.Empty : System.Text.Encoding.UTF8.GetString(www.bytes, 0, www.bytes.Length - 1));
                return;
            }
            object desObject = null;
            try
            {
                if (DataType == EHttpDataType.Protobuf)
                {
                    MemoryStream stream = new MemoryStream(www.bytes);
                    stream.SetLength(www.bytes.Length);
                    desObject = RuntimeTypeModel.Default.Deserialize(stream, null, ResponseType);
                }
                else if (DataType == EHttpDataType.Json)
                {
                    string jsonString = System.Text.Encoding.UTF8.GetString(www.bytes);
                    desObject = JsonMapper.ToObject(jsonString, ResponseType);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                InvokeFinished(httpImpl, HttpErrorCode.DataTypeError, null, string.Empty);
                return;
            }
            InvokeFinished(httpImpl, HttpErrorCode.OK, desObject, string.Empty);
        }
        public void Dispose()
        {
            if (www != null)
            {
                www.Dispose();
                www = null;
            }
        }
        private HttpErrorCode GetResponseCode()
        {
            if (www == null || www.responseHeaders == null || www.responseHeaders.ContainsKey("STATUS") == false)
            {
                return HttpErrorCode.Exception;
            }
            string[] components = www.responseHeaders["STATUS"].Split(' ');
            int ret;
            if (components.Length >= 3 && int.TryParse(components[1], out ret))
            {
                return (HttpErrorCode)ret;
            }
            return HttpErrorCode.Exception;
        }
        private void InvokeFinished(IHttpManagerImpl httpImpl, HttpErrorCode errorCode, object res, string errorMsg)
        {
            if (OnFinished == null)
            {
                return;
            }
            Debug.Log("request " + URL + ", error code: " + errorCode.ToString() + ", " + errorMsg);
            m_LastErrorMessage = errorMsg;
            if (httpImpl != null)
            {
                httpImpl.OnInvokeHTTPResult(URL, Cmd, errorCode, errorMsg);
            }
            OnFinished(errorCode, res);
        }
    }
}
