using DotNetty.Extensions;
using System;
using System.Text;

namespace TestUdp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var udp = new UdpSocket(8888);

            udp.OnStart(() =>
            {
                Console.WriteLine("UDP服务启动8888");
            });

            udp.OnRecieve((endPoint, bytes) =>
            {
                Console.WriteLine(endPoint);
                Console.WriteLine(Encoding.UTF8.GetString(bytes));
                udp.SendAsync(endPoint, bytes);
            });

            udp.OnException(ex =>
            {
                Console.WriteLine(ex);
            });

            udp.OnStop(ex =>
            {
                Console.WriteLine("Close:" + ex);
                //restart
                //udp.StartAsync();
            });

            udp.StartAsync();

            Console.ReadKey();
        }
    }
}
