using System;
using System.Security.Cryptography.X509Certificates;

namespace DotNetty.Extensions
{
    abstract class BaseGenericClientBuilder<TBuilder, TTarget, TData> :
        BaseBuilder<TBuilder, TTarget>,
        IGenericClientBuilder<TBuilder, TTarget, TData>
        where TBuilder : class
    {
        public BaseGenericClientBuilder(string ip, int port, int idle, X509Certificate2 cert)
            : base(port, idle, cert)
        {
            _ip = ip;
        }

        protected string _ip { get; }


        protected TcpSocketCientEvent<TTarget, TData> _event { get; }
            = new TcpSocketCientEvent<TTarget, TData>();

        public TBuilder OnClientClose(Action<TTarget> action)
        {
            _event.OnClientClose = action;

            return this as TBuilder;
        }

        public TBuilder OnClientStarted(Action<TTarget> action)
        {
            _event.OnClientStarted = action;

            return this as TBuilder;
        }

        public TBuilder OnRecieve(Action<TTarget, TData> action)
        {
            _event.OnRecieve = action;

            return this as TBuilder;
        }

        public TBuilder OnSend(Action<TTarget, TData> action)
        {
            _event.OnSend = action;

            return this as TBuilder;
        }

        public override TBuilder OnException(Action<Exception> action)
        {
            _event.OnException = action;

            return this as TBuilder;
        }
    }
}
