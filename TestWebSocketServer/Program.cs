using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using DotNetty.Extensions;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;

namespace TestWebSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer(8888, useLibuv: true);

            server.OnPipeline(pipeline =>
            {
                //心跳
                //pipeline.AddLast(new IdleStateHandler(5, 0, 0));

                //tls证书
                //var cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
                //pipeline.AddLast(TlsHandler.Server(cert));

            });

            server.OnStart(() =>
            {
                Console.WriteLine("服务启动成功");
            });

            server.OnConnectionConnect(conn =>
            {
                Console.WriteLine("OnConnectionConnect:" + conn.Id);
                Console.WriteLine("当前连接数:" + server.GetConnectionCount());
                conn.SendTextAsync("嘿,欢迎来到服务器");
            });

            server.OnConnectionReceiveText((conn, text) =>
            {
                Console.WriteLine("OnConnectionReceiveText:" + text);
            });

            server.OnConnectionReceiveBinary((conn, bytes) =>
            {
                Console.WriteLine("OnConnectionReceiveBinary:" + bytes);
            });

            server.OnConnectionException((conn, ex) =>
            {
                Console.WriteLine("OnConnectionException:" + ex);
            });

            server.OnConnectionClose(conn =>
            {
                Console.WriteLine("OnConnectionClose:" + conn.Id);
                Console.WriteLine("当前连接数:" + server.GetConnectionCount());
            });

            server.OnStop(ex =>
            {
                Console.WriteLine(ex);
                //restart
                //server.StartAsync();
            });

            server.StartAsync();

            Console.ReadKey();
        }
    }
}
