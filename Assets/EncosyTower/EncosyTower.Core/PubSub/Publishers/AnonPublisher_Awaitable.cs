#if !UNITASK && UNITY_6000_0_OR_NEWER

using EncosyTower.Pooling;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;
using UnityEngine;

namespace EncosyTower.PubSub
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
