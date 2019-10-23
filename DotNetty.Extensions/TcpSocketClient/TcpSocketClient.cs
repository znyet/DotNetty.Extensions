using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    public class TcpSocketClient
    {
        private string _serverIp;

        private int _serverPort;

        public TcpSocketClient(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        private IEventLoopGroup group;

        private Bootstrap bootstrap;

        internal IChannel channel;

        internal IChannel channelWork;

        internal TcpClientEvent _event = new TcpClientEvent();

        public async Task ConnectAsync()
        {
            if (group == null)
            {
                group = new MultithreadEventLoopGroup();
            }

            if (bootstrap == null)
            {
                bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(ch =>
                    {
                        IChannelPipeline pipeline = ch.Pipeline;
                        _event.OnPipelineAction?.Invoke(pipeline);
                        pipeline.AddLast(new TcpClientHandler(this));

                    }));
            }

            await Close();

            try
            {
                channelWork = await bootstrap.ConnectAsync(IPAddress.Parse(_serverIp), _serverPort);
            }
            catch (Exception ex)
            {
                _event.OnCloseAction?.Invoke(ex);
            }
        }

        private async Task Close()
        {
            if (channelWork != null)
            {
                await channelWork.CloseAsync();
            }

            if (channel != null)
            {
                await channel.CloseAsync();
            }

        }

        public async Task CloseAsync()
        {
            await Close();
            _event.OnCloseAction?.Invoke(new Exception("CloseAsync"));
        }

        public async Task ShutdownAsync()
        {
            await Close();
            if (group != null)
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
            bootstrap = null;
            group = null;
            _event.OnCloseAction?.Invoke(new Exception("ShutdownAsync"));
        }

        public void OnPipeline(Action<IChannelPipeline> action)
        {
            _event.OnPipelineAction = action;
        }


        public async Task SendAsync(byte[] bytes)
        {
            await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(bytes));
        }

        public void OnConnect(Action action)
        {
            _event.OnConnectAction = action;
        }

        public void OnReceive(Action<byte[]> action)
        {
            _event.OnReceiveAction = action;
        }

        public void OnException(Action<Exception> action)
        {
            _event.OnExceptionAction = action;
        }

        public void OnClose(Action<Exception> action)
        {
            _event.OnCloseAction = action;
        }



    }
}
