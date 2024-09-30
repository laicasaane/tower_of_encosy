#if UNITASK

using Cysharp.Threading.Tasks;
using Module.Core.Collections;
using Module.Core.PubSub.Internals;
using Module.Core.Vaults;

namespace Module.Core.PubSub
{
    partial class MessageSubscriber
    {
        private readonly CappedArrayPool<UniTask> _taskArrayPool;

        internal MessageSubscriber(
              SingletonVault<MessageBroker> brokers
            , CappedArrayPool<UniTask> taskArrayPool
        )
        {
            _brokers = brokers;
            _taskArrayPool = taskArrayPool;
        }
    }
}

#endif
