using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    static class Extention
    {
        /// <summary>
        /// 转为网络终结点IPEndPoint
        /// </summary>=
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static IPEndPoint ToIPEndPoint(this string str)
        {
            IPEndPoint iPEndPoint = null;
            try
            {
                string[] strArray = str.Split(':').ToArray();
                string addr = strArray[0];
                int port = Convert.ToInt32(strArray[1]);
                iPEndPoint = new IPEndPoint(IPAddress.Parse(addr), port);
            }
            catch
            {
                iPEndPoint = null;
            }

            return iPEndPoint;
        }

        /// <summary>
        /// 获取IByteBuffer中的byte[]
        /// </summary>
        /// <param name="byteBuffer">IByteBuffer</param>
        /// <returns></returns>
        public static byte[] ToBytes(this IByteBuffer byteBuffer)
        {
            int readableBytes = byteBuffer.ReadableBytes;
            if (readableBytes == 0)
            {
                return ArrayExtensions.ZeroBytes;
            }

            //if (byteBuffer.HasArray)
            //{
            //    return byteBuffer.Array.Slice(byteBuffer.ArrayOffset + byteBuffer.ReaderIndex, readableBytes);

            //}

            var bytes = new byte[readableBytes];
            byteBuffer.GetBytes(byteBuffer.ReaderIndex, bytes);
            return bytes;
        }

        public static string GetWebSocketLocation(IFullHttpRequest req, string path, bool useSsl)
        {
            bool result = req.Headers.TryGet(HttpHeaderNames.Host, out ICharSequence value);
            string location = value.ToString() + path;
            if (useSsl)
            {
                return "wss://" + location;
            }
            else
            {
                return "ws://" + location;
            }
        }

        public static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        {
            // Generate an error page if response getStatus code is not OK (200).
            if (res.Status.Code != 200)
            {
                IByteBuffer buf = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(res.Status.ToString()));
                res.Content.WriteBytes(buf);
                buf.Release();
                HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
            }

            // Send the response and close the connection if necessary.
            Task task = ctx.Channel.WriteAndFlushAsync(res);
            if (!HttpUtil.IsKeepAlive(req) || res.Status.Code != 200)
            {
                task.ContinueWith((t, c) => ((IChannelHandlerContext)c).CloseAsync(),
                    ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }


    }
}
