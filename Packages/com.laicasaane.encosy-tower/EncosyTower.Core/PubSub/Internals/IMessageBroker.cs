#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Logging;

namespace EncosyTower.PubSub.Internals
{
    internal interface IMessageBroker : IDisposable
    {
        void Compress(ILogger logger);
    }
}

#endif
