#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Buffers;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    partial class MessageSubscriber
    {
        private readonly ArrayPool<UnityTask> _taskArrayPool;

        internal MessageSubscriber(
              SingletonVault<MessageBroker> brokers
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            _brokers = brokers;
            _taskArrayPool = taskArrayPool;
        }
    }
}

#endif
