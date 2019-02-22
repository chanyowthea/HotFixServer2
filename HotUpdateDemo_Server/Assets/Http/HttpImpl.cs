using GCommon;
using message;
using System;
using UnityEngine;

namespace COW
{
    public class HttpImpl : IHttpManagerImpl
    {
        public HttpImpl()
        {
            HttpManager.instance.SetImpl(this);
        }

        ~HttpImpl()
        {
            HttpManager.instance.SetImpl(null);
        }

        public void OnStartRequest(HttpRequest curHttpReq)
        {
            if (curHttpReq.ResponseType != null) //need waiting for response
            {
                // TODO: remove later
                if (curHttpReq.HasRetried())
                    Debug.LogError("br start request and show waiting ui " + curHttpReq.ResponseType);

            }
        }
        public void OnEndRequest(HttpRequest nextHttpReq)
        {
            if (nextHttpReq == null) //no pending request
            {
                //Debugger.LogError("br start request and hide waiting ui ");
            }
        }
        public void OnUnauthorized()
        {

        }

        public void OnInvokeHTTPResult(string url, string cmd, HttpErrorCode errorCode, string errorMsg)
        {
            if (errorCode != HttpErrorCode.Timeout)
            {
                return;
            }
        }

        public void OnRetryFailed(HttpRequest curHttpReq)
        {

        }
    }
}
