using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    public class WebSocketClient
    {
        public WebSocketClient(string uri)
        {
            builder = new UriBuilder(uri);
        }

        private UriBuilder builder;

        private IEventLoopGroup group;

        private Bootstrap bootstrap;

        internal IChannel channel;

        internal IChannel channelWork;

        internal WebSocketClientEvent _event = new WebSocketClientEvent();

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
                    .Handler(new ActionChannelInitializer<IChannel>(ch =>
                    {
                        IChannelPipeline pipeline = ch.Pipeline;
                        _event.OnPipelineAction?.Invoke(pipeline);
                        pipeline.AddLast(new HttpClientCodec());
                        pipeline.AddLast(new HttpObjectAggregator(8192));
                        pipeline.AddLast(WebSocketClientCompressionHandler.Instance);
                        pipeline.AddLast(new WebSocketClientHandler(this, WebSocketClientHandshakerFactory.NewHandshaker(builder.Uri, WebSocketVersion.V13, null, true, new DefaultHttpHeaders())));
                    }));

            }

            await Close();

            try
            {
                channelWork = await bootstrap.ConnectAsync(IPAddress.Parse(builder.Host), builder.Port);
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


        public async Task SendTextAsync(string text)
        {
            var frame = new TextWebSocketFrame(text);
            await channel.WriteAndFlushAsync(frame);
        }

        public async Task SendBinaryAsync(byte[] bytes)
        {
            var frame = new BinaryWebSocketFrame(Unpooled.WrappedBuffer(bytes));
            await channel.WriteAndFlushAsync(frame);
        }

        public void OnConnect(Action action)
        {
            _event.OnConnectAction = action;
        }

        public void OnReceiveText(Action<string> action)
        {
            _event.OnReceiveTextAction = action;
        }

        public void OnReceiveBinary(Action<byte[]> action)
        {
            _event.OnReceiveBinaryAction = action;
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
