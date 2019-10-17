namespace DotNetty.Extensions
{
    /// <summary>
    /// TcpSocket客户端构建者
    /// </summary>
    public interface ITcpSocketClientBuilder :
        IGenericClientBuilder<ITcpSocketClientBuilder, ITcpSocketClient, byte[]>,
        ICoderBuilder<ITcpSocketClientBuilder>
    {

    }
}
