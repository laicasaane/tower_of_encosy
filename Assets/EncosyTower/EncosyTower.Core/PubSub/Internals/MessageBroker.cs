#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Logging;

namespace EncosyTower.PubSub.Internals
{
    internal abstract class MessageBroker : IDisposable
    {
        public abstract void Dispose();

        public abstract void Compress(ILogger logger);
    }
}

#endif
