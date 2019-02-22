using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

class HttpServer : HttpService
{
    Thread thread;

    public HttpServer(int port)
    {
        host = string.Format("http://localhost:{0}/", port);
    }

    public void Start()
    {
        Console.WriteLine("Server Start! host=" + host); 
        thread = new Thread(new ThreadStart(Listen));
        thread.Start();

        Console.WriteLine(AssetPath); 
    }

    public new void Stop()
    {
        base.Stop();
        thread.Abort();
    }

    string AssetPath
    {
        get
        {
            string exePath = Environment.CurrentDirectory;
            string parent = Directory.GetParent(exePath).Parent.Parent.FullName + @"\HotUpdateDemo_Server\Assets\StreamingAssets\";
            parent = parent.Replace('\\', '/');
            return parent;
        }
    }

    string GetMimeType(string file)
    {
        string extName = Path.GetExtension(file.ToLower());
        switch (extName)
        {
            case ".png": return "image/png";
            case ".jpg": return "image/jpeg";
            case ".txt": return "text/plain";
            default: return "application/octet-stream";
        }
    }

    public override void OnGetRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        Console.WriteLine("Url=" + request.Url);
        string url = request.Url.ToString().Remove(0, host.Length);
        if (url.Contains("?"))
        {
            url = url.Substring(0, url.IndexOf("?")); // ?v
        }
        string filename = AssetPath + url;
        string responseString = string.Empty;
        if (url.EndsWith("/"))
        {
            responseString = "<html><body><h1>SimpleFramework WebServer 0.1.0</h1>";
            responseString += "Current Time: " + DateTime.Now.ToString() + "<br>";
            responseString += "url : " + request.Url + "<br>";
            responseString += "Asset Path: " + filename;
            Label(response, responseString);
        }
        else
        {
            if (File.Exists(filename))
            {
                using (Stream fs = File.Open(filename, FileMode.Open))
                {
                    response.ContentLength64 = fs.Length;
                    response.ContentType = GetMimeType(filename) + "; charset=UTF-8";

                    fs.CopyTo(response.OutputStream);
                    response.OutputStream.Flush();
                    response.Close(); return;
                }
            }
            else
            {
                if (url == "ver.php")
                {
                    // 设置数据
                    HttpVerInfo res = new HttpVerInfo();
                    res.is_server_open = true; 
                    res.cdn_url = "http://localhost:8080/"; 
                    ProtoBuf.Serializer.Serialize(response.OutputStream, res);
                    response.OutputStream.Flush();
                    response.OutputStream.Close();
                }
                else
                {
                    responseString = "<h1>404</h1>";
                    Label(response, responseString); 
                }
            }
        }
    }

    void Label(HttpListenerResponse response, string responseString)
    {
        try
        {
            response.ContentLength64 = Encoding.UTF8.GetByteCount(responseString);
            response.ContentType = "text/html; charset=UTF-8";
        }
        finally
        {
            Stream output = response.OutputStream;
            StreamWriter writer = new StreamWriter(output);
            writer.Write(responseString);
            writer.Close();
        }
    }

    private const int BUF_SIZE = 4096;
    private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
    public override void OnPostRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        Console.WriteLine("POST request: {0}", request.Url);
        MemoryStream ms = new MemoryStream();
        int content_len = 0;
        if (request.Headers.Get("Content-Length") != null)
        {
            content_len = Convert.ToInt32(request.Headers.Get("Content-Length"));
            if (content_len > MAX_POST_SIZE)
            {
                throw new Exception(
                    String.Format("POST Content-Length({0}) too big for this simple server",
                      content_len));
            }
            byte[] buf = new byte[BUF_SIZE];
            int to_read = content_len;
            while (to_read > 0)
            {
                Console.WriteLine("starting Read, to_read={0}", to_read);
                int numread = request.InputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                Console.WriteLine("read finished, numread={0}", numread);
                if (numread == 0)
                {
                    if (to_read == 0)
                    {
                        break;
                    }
                    else
                    {
                        throw new Exception("client disconnected during post");
                    }
                }
                to_read -= numread;
                ms.Write(buf, 0, numread);
            }
            ms.Seek(0, SeekOrigin.Begin);
        }
        Console.WriteLine("get post data end");
        var writer = new StreamWriter(response.OutputStream); 
        writeSuccess(writer);

        // 解析数据
        var req = ProtoBuf.Serializer.Deserialize<CreateClanReq>(ms);
        if (req != null)
        {
            Console.WriteLine("CreateClanReq name=" + req._name); 
        }

        // 设置数据
        CreateClanRes res = new CreateClanRes();
        res._ids = new int[2] { 0, 1 }; 
        ProtoBuf.Serializer.Serialize(response.OutputStream, res);

        response.OutputStream.Flush();
        response.OutputStream.Close(); 
    }

    public void writeSuccess(StreamWriter s)
    {
        s.WriteLine("HTTP/1.0 200 OK");
        s.WriteLine("Content-Type: text/html");
        s.WriteLine("Connection: close");
        s.WriteLine("");
    }

    public void writeFailure(StreamWriter s)
    {
        s.WriteLine("HTTP/1.0 404 File not found");
        s.WriteLine("Connection: close");
        s.WriteLine("");
    }
}
