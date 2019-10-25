using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    public class TcpSocketServer
    {
        private int _port;

        private IPAddress _ipAddress;

        private bool _useLibuv;

        private int _soBacklog;

        public TcpSocketServer(int port, IPAddress ipAddress = null, int soBacklog = 1024, bool useLibuv = false)
        {
            _port = port;
            _ipAddress = ipAddress;
            _useLibuv = useLibuv;
            _soBacklog = soBacklog;
        }

        private IEventLoopGroup bossGroup;

        private IEventLoopGroup workerGroup;

        private ServerBootstrap bootstrap;

        private IChannel channel;

        private TcpServerEvent _event = new TcpServerEvent();

        private readonly ConcurrentDictionary<string, TcpSocketConnection> connectionDict = new ConcurrentDictionary<string, TcpSocketConnection>();

        public async Task StartAsync()
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
                }
                else
                {
                    bootstrap.Channel<TcpServerSocketChannel>();
                }

                bootstrap
                    .Option(ChannelOption.SoBacklog, _soBacklog)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(ch =>
                    {
                        IChannelPipeline pipeline = ch.Pipeline;
                        _event.OnPipelineAction?.Invoke(pipeline);
                        pipeline.AddLast(new TcpServerHandler(_event, connectionDict));

                    }));
            }

            if (_ipAddress == null)
            {
                _ipAddress = IPAddress.Any;
            }

            await Stop();

            try
            {
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

        public ICollection<TcpSocketConnection> GetAllConnection()
        {
            return connectionDict.Values;
        }

        public IEnumerable<string> GetAllConnectionName()
        {
            return connectionDict.Values.Select(s => s.Name);
        }

        public TcpSocketConnection GetConnectionById(string id)
        {
            connectionDict.TryGetValue(id, out TcpSocketConnection connection);
            return connection;
        }

        public IEnumerable<TcpSocketConnection> GetConnectionByName(string name)
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

        public void OnConnectionConnect(Action<TcpSocketConnection> action)
        {
            _event.OnConnectionConnectAction = action;
        }

        public void OnConnectionReceive(Action<TcpSocketConnection, byte[]> action)
        {
            _event.OnConnectionReceiveAction = action;
        }

        public void OnConnectionException(Action<TcpSocketConnection, Exception> action)
        {
            _event.OnConnectionExceptionAction = action;
        }

        public void OnConnectionClose(Action<TcpSocketConnection> action)
        {
            _event.OnConnectionCloseAction = action;
        }

        public void OnStop(Action<Exception> action)
        {
            _event.OnStopAction = action;
        }

    }
}
