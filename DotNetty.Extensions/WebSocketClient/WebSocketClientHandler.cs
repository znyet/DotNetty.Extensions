using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    class WebSocketClientHandler : SimpleChannelInboundHandler<object>
    {
        private WebSocketClient _client;

        readonly WebSocketClientHandshaker handshaker;

        readonly TaskCompletionSource completionSource;

        public WebSocketClientHandler(WebSocketClient client, WebSocketClientHandshaker shaker)
        {
            _client = client;

            handshaker = shaker;
            completionSource = new TaskCompletionSource();
        }

        public Task HandshakeCompletion => completionSource.Task;

        public override void ChannelActive(IChannelHandlerContext context)
        {
            handshaker.HandshakeAsync(context.Channel).LinkOutcome(completionSource);
            _client.channel = context.Channel;
        }

        protected override void ChannelRead0(IChannelHandlerContext context, object msg)
        {

            IChannel ch = context.Channel;
            if (!handshaker.IsHandshakeComplete)
            {
                try
                {
                    handshaker.FinishHandshake(ch, (IFullHttpResponse)msg);
                    completionSource.TryComplete();
                    _client._event.OnConnectAction?.Invoke();
                }
                catch (WebSocketHandshakeException e)
                {
                    completionSource.TrySetException(e);
                }

                return;
            }

            if (msg is IFullHttpResponse response)
            {
                throw new InvalidOperationException($"Unexpected FullHttpResponse (getStatus={response.Status}, content={response.Content.ToString(Encoding.UTF8)})");
            }

            if (msg is TextWebSocketFrame textFrame)
            {
                _client._event.OnReceiveTextAction?.Invoke(textFrame.Text());
            }
            else if (msg is BinaryWebSocketFrame binaryFrame)
            {
                _client._event.OnReceiveBinaryAction?.Invoke(binaryFrame.Content.ToBytes());
            }
            else if (msg is PongWebSocketFrame)
            {

            }
            else if (msg is CloseWebSocketFrame)
            {
                ch.CloseAsync();
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _client._event.OnExceptionAction?.Invoke(exception);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _client._event.OnCloseAction.Invoke(new Exception("ChannelInactive"));
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent)
            {
                var eventState = evt as IdleStateEvent;
                if (eventState != null)
                {
                    context.Channel.CloseAsync();
                }

            }
        }
    }
}
