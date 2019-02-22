using System;
using System.Net;

abstract class HttpService
{
    protected string host;
    HttpListener listener;
    bool is_active = true;

    public HttpService()
    {
    }

    public void Listen()
    {
        Console.WriteLine("Listen");
        if (!HttpListener.IsSupported)
        {
            throw new System.InvalidOperationException(
                "使用 HttpListener 必须为 Windows XP SP2 或 Server 2003 以上系统！");
        }
        string[] prefixes = new string[] { host };

        Console.WriteLine("host=" + host);
        listener = new HttpListener();
        foreach (string s in prefixes)
        {
            listener.Prefixes.Add(s);
        }
        listener.Start();
        Console.WriteLine("is_active=" + is_active);

        while (is_active)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            Console.WriteLine("{0} {1} HTTP/1.1", request.HttpMethod, request.RawUrl);
            Console.WriteLine("User-Agent: {0}", request.UserAgent);
            Console.WriteLine("Accept-Encoding: {0}", request.Headers["Accept-Encoding"]);
            Console.WriteLine("Connection: {0}", request.KeepAlive ? "Keep-Alive" : "close");
            Console.WriteLine("Host: {0}", request.UserHostName);

            HttpListenerResponse response = context.Response;
            Console.WriteLine("method=" + request.HttpMethod); 
            if (request.HttpMethod == "GET")
            {
                OnGetRequest(request, response);
            }
            else
            {
                OnPostRequest(request, response);
            }
        }
    }

    public void Stop()
    {
        is_active = false;
        listener.Stop();
    }

    public abstract void OnGetRequest(HttpListenerRequest request, HttpListenerResponse response);
    public abstract void OnPostRequest(HttpListenerRequest request, HttpListenerResponse response);
}
