#if UNITASK

using Cysharp.Threading.Tasks;
using EncosyTower.Pooling;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
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
