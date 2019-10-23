using DotNetty.Extensions;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestUdp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var udp = new UdpSocket(7777);

            udp.OnStart(() =>
            {
                Console.WriteLine("UDP服务启动7777");
                var endPiont = new IPEndPoint(IPAddress.Broadcast, 8888);
                var bytes = Encoding.UTF8.GetBytes("您好");
                udp.SendAsync(endPiont, bytes);
            });

            udp.OnRecieve(async (endPoint, bytes) =>
            {
                Console.WriteLine(endPoint);
                Console.WriteLine(Encoding.UTF8.GetString(bytes));
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
