using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Extensions;
using DotNetty.Handlers.Timeout;

namespace TestTcpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpSocketClient("127.0.0.1", 8888);

            client.OnPipeline(pipeline =>
            {
                //心跳
                //pipeline.AddLast(new IdleStateHandler(5, 0, 0));

                //编码解码器
                //pipeline.AddLast(new LengthFieldPrepender(2));
                //pipeline.AddLast(new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                //tls证书
                //var cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
                //var targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
                //pipeline.AddLast(new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));

            });

            client.OnConnect(() =>
            {
                Console.WriteLine("OnConnect");
                var bytes = Encoding.UTF8.GetBytes("Hello Word");
                client.SendAsync(bytes);
            });

            client.OnReceive(bytes =>
            {
                Console.WriteLine("OnReceive:" + bytes);
            });

            client.OnException(ex =>
            {
                Console.WriteLine("OnException:" + ex);

            });

            client.OnClose(ex =>
            {
                Console.WriteLine("OnClose:" + ex);
                //restart
                //client.ConnectAsync();
            });

            client.ConnectAsync();

            Console.ReadKey();
        }
    }
}
