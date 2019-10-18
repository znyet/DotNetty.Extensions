using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    abstract class BaseBuilder<TBuilder, TTarget> : IBuilder<TBuilder, TTarget> where TBuilder : class
    {
        public BaseBuilder(int port, int idle, X509Certificate2 cert)
        {
            _port = port;
            _idle = idle;
            _cert = cert;
        }
        protected int _port { get; }

        protected int _idle { get; }

        protected X509Certificate2 _cert { get; }

        public abstract Task<TTarget> BuildAsync();

        public abstract TBuilder OnException(Action<Exception> action);
    }
}
