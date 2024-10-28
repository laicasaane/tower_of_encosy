#if !UNITASK && UNITY_6000_0_OR_NEWER

using EncosyTower.Modules.Collections;
using EncosyTower.Modules.PubSub.Internals;
using EncosyTower.Modules.Vaults;
using UnityEngine;

namespace EncosyTower.Modules.PubSub
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
