using System;

namespace Core
{
    public interface IFlushable : IDisposable
    {
        void ForceFlushAll();
    }
}
