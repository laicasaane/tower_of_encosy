#if !UNITASK && UNITY_6000_0_OR_NEWER

using EncosyTower.Modules.Collections;
using EncosyTower.Modules.PubSub.Internals;
using EncosyTower.Modules.Vaults;
using UnityEngine;

namespace EncosyTower.Modules.PubSub
{
    public partial class MessagePublisher
    {
        private readonly CappedArrayPool<Awaitable> _taskArrayPool;

        internal MessagePublisher(
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
