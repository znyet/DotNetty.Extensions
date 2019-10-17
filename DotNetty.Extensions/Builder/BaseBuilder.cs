using System;
using System.Threading.Tasks;

namespace DotNetty.Extensions
{
    abstract class BaseBuilder<TBuilder, TTarget> : IBuilder<TBuilder, TTarget> where TBuilder : class
    {
        public BaseBuilder(int port,int idle)
        {
            _port = port;
            _idle = idle;
        }
        protected int _port { get; }

        protected int _idle { get; }

        public abstract Task<TTarget> BuildAsync();

        public abstract TBuilder OnException(Action<Exception> action);
    }
}
