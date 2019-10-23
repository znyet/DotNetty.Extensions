using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;
using static DotNetty.Codecs.Http.HttpResponseStatus;
using static DotNetty.Codecs.Http.HttpVersion;

namespace DotNetty.Extensions
{
    class WebSocketServerHandler : SimpleChannelInboundHandler<object>
    {
        private WebSocketServer _server;

        public WebSocketServerHandler(WebSocketServer server)
        {
            _server = server;
        }

        private WebSocketServerHandshaker handshaker;

        private void HandleHttpRequest(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            // Handle a bad request.
            if (!req.Result.IsSuccess)
            {
                Extention.SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, BadRequest));
                return;
            }

            // Allow only GET methods.
            if (!Equals(req.Method, HttpMethod.Get))
            {
                Extention.SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, Forbidden));
                return;
            }

            if ("/favicon.ico".Equals(req.Uri))
            {
                var res = new DefaultFullHttpResponse(Http11, NotFound);
                Extention.SendHttpResponse(ctx, req, res);
                return;
            }

            // Handshake
            var wsFactory = new WebSocketServerHandshakerFactory(Extention.GetWebSocketLocation(req, _server._path, _server._useSsl), null, true, 5 * 1024 * 1024);

            handshaker = wsFactory.NewHandshaker(req);

            if (handshaker == null)
            {
                WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
            }
            else
            {
                handshaker.HandshakeAsync(ctx.Channel, req);
            }

            _server.connectionDict.TryGetValue(ctx.Channel.Id.AsShortText(), out WebSocketConnection conn);
            _server._event.OnConnectionConnectAction?.Invoke(conn);
        }

        private void HandleWebSocketFrame(IChannelHandlerContext ctx, WebSocketFrame frame)
        {
            // Check for closing frame
            if (frame is CloseWebSocketFrame)
            {
                handshaker.CloseAsync(ctx.Channel, (CloseWebSocketFrame)frame.Retain());
                return;
            }

            if (frame is PingWebSocketFrame)
            {
                ctx.WriteAsync(new PongWebSocketFrame((IByteBuffer)frame.Content.Retain()));
                return;
            }

            if (frame is TextWebSocketFrame textFrame)
            {
                _server.connectionDict.TryGetValue(ctx.Channel.Id.AsShortText(), out WebSocketConnection conn);
                _server._event.OnConnectionReceiveTextAction?.Invoke(conn, textFrame.Text());
                return;
            }

            if (frame is BinaryWebSocketFrame)
            {
                _server.connectionDict.TryGetValue(ctx.Channel.Id.AsShortText(), out WebSocketConnection conn);
                _server._event.OnConnectionReceiveBinaryAction?.Invoke(conn, frame.Content.ToBytes());
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var conn = new WebSocketConnection(context.Channel);
            _server.connectionDict.TryAdd(context.Channel.Id.AsShortText(), conn);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            if (msg is IFullHttpRequest request)
            {
                HandleHttpRequest(ctx, request);
            }
            else if (msg is WebSocketFrame frame)
            {
                HandleWebSocketFrame(ctx, frame);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            _server.connectionDict.TryGetValue(ctx.Channel.Id.AsShortText(), out WebSocketConnection conn);
            _server._event.OnConnectionExceptionAction?.Invoke(conn, e);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _server.connectionDict.TryRemove(context.Channel.Id.AsShortText(), out WebSocketConnection conn);
            _server._event.OnConnectionCloseAction?.Invoke(conn);
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
