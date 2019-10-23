using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Extensions;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;

namespace TestWebSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new WebSocketClient("ws://127.0.0.1:8888");

            client.OnPipeline(pipeline =>
            {
                //心跳
                //pipeline.AddLast(new IdleStateHandler(5, 0, 0));

                //tls证书
                //var cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
                //var targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
                //pipeline.AddLast(new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));

            });

            client.OnConnect(() =>
            {
                Console.WriteLine("OnConnect");
                client.SendTextAsync("Hello Word");
                client.SendBinaryAsync(new byte[] { 1 });

            });

            client.OnReceiveText(text =>
            {
                Console.WriteLine("OnReceiveText:" + text);
            });

            client.OnReceiveBinary(bytes =>
            {
                Console.WriteLine("OnReceiveBinary:" + bytes);
            });

            client.OnException(ex =>
            {
                Console.WriteLine("OnException:" + ex);

            });

            client.OnClose(ex =>
            {
                Console.WriteLine("OnClose:" + ex);
                //restart
                client.ConnectAsync();
            });

            client.ConnectAsync();


            Console.ReadKey();
        }
    }
}
