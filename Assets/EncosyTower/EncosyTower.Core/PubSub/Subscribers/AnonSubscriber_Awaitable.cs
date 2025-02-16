#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using EncosyTower.PubSub.Internals;
using EncosyTower.Pooling;
using EncosyTower.Vaults;
using UnityEngine;

namespace EncosyTower.PubSub
{
    partial class AnonSubscriber
    {
        internal AnonSubscriber(
              SingletonVault<MessageBroker> brokers
            , CappedArrayPool<Awaitable> taskArrayPool
        )
        {
            _subscriber = new(brokers, taskArrayPool);
        }
    }
}

#endif
