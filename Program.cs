using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"D:\FF\Test\HotFixServer2\bin\Debug\Files\";
            //// D:\FF\Test\HotFixServer2\bin\Debug\Files\HotUpdateDemo_Server\Assets\StreamingAssets\AssetBundles
            //using (Stream fs = File.Open(path + "HotUpdateDemo_Server/Assets/StreamingAssets/AssetBundles/bot", FileMode.Open))
            //{
            //    Console.WriteLine("fs.Length=" + fs.Length); 
            //}
            //Console.ReadKey(); 

            Init();
        }

        private static HttpServer http;
        /// <summary>
        /// 服务器初始化
        /// </summary>
        public static void Init()
        {
            http = new HttpServer(8080);
            http.Start();
        }

        /// <summary>
        /// 服务器关闭
        /// </summary>
        public static void Close()
        {
            http.Stop(); http = null;
        }
    }
}
