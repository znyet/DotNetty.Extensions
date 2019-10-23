using DotNetty.Transport.Channels;
using System;

namespace DotNetty.Extensions
{
    class WebSocketClientEvent
    {
        internal Action<IChannelPipeline> OnPipelineAction;

        internal Action OnConnectAction;

        internal Action<string> OnReceiveTextAction;

        internal Action<byte[]> OnReceiveBinaryAction;

        internal Action<Exception> OnExceptionAction;

        internal Action<Exception> OnCloseAction;
    }
}
