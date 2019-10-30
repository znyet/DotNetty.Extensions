using DotNetty.Codecs.Http;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    public class WebSocketServer
    {
        private int _port;

        private IPAddress _ipAddress;

        internal string _path;

        internal bool _useSsl;

        private bool _useLibuv;


        public WebSocketServer(int port, IPAddress ipAddress = null, string path = "/", bool useLibuv = false)
        {
            _port = port;
            _ipAddress = ipAddress;
            _path = path;
            _useLibuv = useLibuv;

        }

        private IEventLoopGroup bossGroup;

        private IEventLoopGroup workerGroup;

        private ServerBootstrap bootstrap;

        private IChannel channel;

        internal WebSocketServerEvent _event = new WebSocketServerEvent();

        internal readonly ConcurrentDictionary<string, WebSocketConnection> connectionDict = new ConcurrentDictionary<string, WebSocketConnection>();

        public async Task StartAsync()
        {
            try
            {
                if (bossGroup == null && workerGroup == null)
                {
                    if (_useLibuv)
                    {
                        var dispatcher = new DispatcherEventLoopGroup();
                        bossGroup = dispatcher;
                        workerGroup = new WorkerEventLoopGroup(dispatcher);
                    }
                    else
                    {
                        bossGroup = new MultithreadEventLoopGroup();
                        workerGroup = new MultithreadEventLoopGroup();
                    }
                }

                if (bootstrap == null)
                {
                    bootstrap = new ServerBootstrap();
                    bootstrap.Group(bossGroup, workerGroup);

                    if (_useLibuv)
                    {
                        bootstrap.Channel<TcpServerChannel>();
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                            || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            bootstrap
                                .Option(ChannelOption.SoReuseport, true)
                                .ChildOption(ChannelOption.SoReuseaddr, true);
                        }
                    }
                    else
                    {
                        bootstrap.Channel<TcpServerSocketChannel>();
                    }

                    bootstrap
                        .Option(ChannelOption.SoBacklog, 8192)
                        .ChildHandler(new ActionChannelInitializer<IChannel>(ch =>
                        {
                            IChannelPipeline pipeline = ch.Pipeline;
                            _event.OnPipelineAction?.Invoke(pipeline);
                            var tls = pipeline.FirstOrDefault(f => f.GetType() == typeof(TlsHandler));
                            if (tls != null)
                            {
                                _useSsl = true;
                            }
                            pipeline.AddLast(new HttpServerCodec());
                            pipeline.AddLast(new HttpObjectAggregator(65536));
                            pipeline.AddLast(new WebSocketServerHandler(this));

                        }));
                }

                if (_ipAddress == null)
                {
                    _ipAddress = IPAddress.Any;
                }

                await Stop();


                channel = await bootstrap.BindAsync(_ipAddress, _port);
                _event.OnStartAction?.Invoke();
            }
            catch (Exception ex)
            {
                _event.OnStopAction?.Invoke(ex);
            }
        }

        private async Task Stop()
        {
            if (channel != null)
            {
                await channel.CloseAsync();
                await Task.WhenAll(connectionDict.Values.Select(s => s.CloseAsync()).ToArray());
                connectionDict.Clear();
            }
        }


        public async Task StopAsync()
        {
            await Stop();
            _event.OnStopAction?.Invoke(new Exception("StopAsync"));
        }

        public async Task ShutdownAsync()
        {
            await Stop();
            var task1 = bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            var task2 = workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            await Task.WhenAll(task1, task2);
            bossGroup = null;
            workerGroup = null;
            bootstrap = null;
            _event.OnStopAction?.Invoke(new Exception("ShutdownAsync"));
        }

        public ICollection<WebSocketConnection> GetAllConnection()
        {
            return connectionDict.Values;
        }

        public IEnumerable<string> GetAllConnectionName()
        {
            return connectionDict.Values.Select(s => s.Name);
        }

        public WebSocketConnection GetConnectionById(string id)
        {
            connectionDict.TryGetValue(id, out WebSocketConnection connection);
            return connection;
        }

        public IEnumerable<WebSocketConnection> GetConnectionByName(string name)
        {
            return connectionDict.Values.Where(w => w.Name == name);
        }

        public int GetConnectionCount()
        {
            return connectionDict.Count;
        }

        public void OnPipeline(Action<IChannelPipeline> action)
        {
            _event.OnPipelineAction = action;
        }

        public void OnStart(Action action)
        {
            _event.OnStartAction = action;
        }

        public void OnConnectionConnect(Action<WebSocketConnection> action)
        {
            _event.OnConnectionConnectAction = action;
        }

        public void OnConnectionReceiveText(Action<WebSocketConnection, string> action)
        {
            _event.OnConnectionReceiveTextAction = action;
        }

        public void OnConnectionReceiveBinary(Action<WebSocketConnection, byte[]> action)
        {
            _event.OnConnectionReceiveBinaryAction = action;
        }

        public void OnConnectionException(Action<WebSocketConnection, Exception> action)
        {
            _event.OnConnectionExceptionAction = action;
        }

        public void OnConnectionClose(Action<WebSocketConnection> action)
        {
            _event.OnConnectionCloseAction = action;
        }

        public void OnStop(Action<Exception> action)
        {
            _event.OnStopAction = action;
        }

    }
}
