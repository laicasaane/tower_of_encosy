#if !UNITASK && UNITY_6000_0_OR_NEWER

using Module.Core.Collections;
using Module.Core.PubSub.Internals;
using Module.Core.Vaults;
using UnityEngine;

namespace Module.Core.PubSub
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
