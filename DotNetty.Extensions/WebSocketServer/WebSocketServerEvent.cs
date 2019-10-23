using DotNetty.Transport.Channels;
using System;

namespace DotNetty.Extensions
{
    class WebSocketServerEvent
    {
        internal Action<IChannelPipeline> OnPipelineAction;

        internal Action OnStartAction;

        internal Action<WebSocketConnection> OnConnectionConnectAction;

        internal Action<WebSocketConnection, string> OnConnectionReceiveTextAction;

        internal Action<WebSocketConnection, byte[]> OnConnectionReceiveBinaryAction;

        internal Action<WebSocketConnection, Exception> OnConnectionExceptionAction;

        internal Action<WebSocketConnection> OnConnectionCloseAction;

        internal Action<Exception> OnStopAction;

    }
}
