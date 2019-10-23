using DotNetty.Buffers;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    public class WebSocketConnection
    {
        private IChannel _channel;

        public WebSocketConnection(IChannel channel)
        {
            _channel = channel;
        }

        public string Id
        {
            get
            {
                if (_channel == null)
                    return null;
                return _channel.Id.AsShortText();
            }
        }

        public bool Open
        {
            get
            {
                if (_channel == null)
                    return false;
                return _channel.Open;
            }
        }

        public string Name { get; set; }

        private ConcurrentDictionary<object, object> _dict;

        public IDictionary<object, object> SessionItems
        {
            get
            {
                if (_dict == null)
                {
                    _dict = new ConcurrentDictionary<object, object>();
                }

                return _dict;
            }
        }

        public async Task SendTextAsync(string text)
        {
            var frame = new TextWebSocketFrame(text);
            await _channel.WriteAndFlushAsync(frame);
        }

        public async Task SendBinaryAsync(byte[] bytes)
        {
            var frame = new BinaryWebSocketFrame(Unpooled.WrappedBuffer(bytes));
            await _channel.WriteAndFlushAsync(frame);
        }

        public async Task CloseAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
            }
        }
    }
}
