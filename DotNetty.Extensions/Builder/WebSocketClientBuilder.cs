using DotNetty.Extensions;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Threading.Tasks;
using DotNetty.Handlers.Timeout;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Handlers.Tls;
using System.Net.Security;

namespace DotNetty.Extensions
{
    class WebSocketClientBuilder : BaseGenericClientBuilder<IWebSocketClientBuilder, IWebSocketClient, string>, IWebSocketClientBuilder
    {
        public WebSocketClientBuilder(string ip, int port, string path, int idle, X509Certificate2 cert)
            : base(ip, port, idle, cert)
        {
            _path = path;
        }
        private string _path { get; }

        public async override Task<IWebSocketClient> BuildAsync()
        {
            WebSocketClient tcpClient = new WebSocketClient(_ip, _port, _path, _event);

            var clientChannel = await new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    if (_cert != null)
                    {
                        var targetHost = _cert.GetNameInfo(X509NameType.DnsName, false);
                        pipeline.AddLast(new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                    }
                    if (_idle != 0)
                    {
                        pipeline.AddLast(new IdleStateHandler(_idle, 0, 0));
                    }
                    pipeline.AddLast(
                        new HttpClientCodec(),
                        new HttpObjectAggregator(8192),
                        new CommonChannelHandler(tcpClient));
                })).ConnectAsync($"{_ip}:{_port}".ToIPEndPoint());
            await tcpClient.HandshakeCompletion;
            tcpClient.SetChannel(clientChannel);

            return await Task.FromResult(tcpClient);
        }
    }
}
