using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    class TcpSocketClientBuilder : BaseGenericClientBuilder<ITcpSocketClientBuilder, ITcpSocketClient, byte[]>, ITcpSocketClientBuilder
    {
        public TcpSocketClientBuilder(string ip, int port, int idle, X509Certificate2 cert)
            : base(ip, port, idle, cert)
        {

        }
        protected Action<IChannelPipeline> _setEncoder { get; set; }

        public ITcpSocketClientBuilder SetLengthFieldDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, ByteOrder byteOrder = ByteOrder.BigEndian)
        {
            _setEncoder += x => x.AddLast(new LengthFieldBasedFrameDecoder(byteOrder, maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, true));

            return this;
        }

        public ITcpSocketClientBuilder SetLengthFieldEncoder(int lengthFieldLength)
        {
            _setEncoder += x => x.AddLast(new LengthFieldPrepender(lengthFieldLength));

            return this;
        }

        public async override Task<ITcpSocketClient> BuildAsync()
        {
            TcpSocketClient tcpClient = new TcpSocketClient(_ip, _port, _event);

            var clientChannel = await new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    _setEncoder?.Invoke(pipeline);
                    if (_cert != null)
                    {
                        var targetHost = _cert.GetNameInfo(X509NameType.DnsName, false);
                        pipeline.AddLast(new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                    }
                    if (_idle != 0)
                    {
                        pipeline.AddLast(new IdleStateHandler(_idle, 0, 0));
                    }
                    pipeline.AddLast(new CommonChannelHandler(tcpClient));
                })).ConnectAsync($"{_ip}:{_port}".ToIPEndPoint());

            tcpClient.SetChannel(clientChannel);

            return await Task.FromResult(tcpClient);
        }
    }
}
