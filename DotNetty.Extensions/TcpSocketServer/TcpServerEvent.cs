using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    class TcpServerEvent
    {
        internal Action<IChannelPipeline> OnPipelineAction;

        internal Action OnStartAction;

        internal Action<TcpSocketConnection> OnConnectionConnectAction;

        internal Action<TcpSocketConnection, byte[]> OnConnectionReceiveAction;

        internal Action<TcpSocketConnection, Exception> OnConnectionExceptionAction;

        internal Action<TcpSocketConnection> OnConnectionCloseAction;

        internal Action<Exception> OnStopAction;

    }
}
