#if !UNITASK && UNITY_6000_0_OR_NEWER

using Module.Core.Collections;
using Module.Core.PubSub.Internals;
using Module.Core.Vaults;
using UnityEngine;

namespace Module.Core.PubSub
{
    partial class AnonPublisher
    {
        internal AnonPublisher(
              SingletonVault<MessageBroker> brokers
            , CappedArrayPool<Awaitable> taskArrayPool
        )
        {
            _publisher = new(brokers, taskArrayPool);
        }
    }
}

#endif
