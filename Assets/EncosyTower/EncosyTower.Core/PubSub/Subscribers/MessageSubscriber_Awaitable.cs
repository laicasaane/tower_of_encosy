#if !UNITASK && UNITY_6000_0_OR_NEWER

using EncosyTower.Pooling;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;
using UnityEngine;

namespace EncosyTower.PubSub
{
    partial class MessageSubscriber
    {
        private readonly CappedArrayPool<Awaitable> _taskArrayPool;

        internal MessageSubscriber(
              SingletonVault<MessageBroker> brokers
            , CappedArrayPool<Awaitable> taskArrayPool
        )
        {
            _brokers = brokers;
            _taskArrayPool = taskArrayPool;
        }
    }
}

#endif
