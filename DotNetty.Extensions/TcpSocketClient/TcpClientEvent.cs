using DotNetty.Transport.Channels;
using System;

namespace DotNetty.Extensions
{
    class TcpClientEvent
    {
        internal Action<IChannelPipeline> OnPipelineAction;

        internal Action OnConnectAction;

        internal Action<byte[]> OnReceiveAction;

        internal Action<Exception> OnExceptionAction;

        internal Action<Exception> OnCloseAction;
    }
}
