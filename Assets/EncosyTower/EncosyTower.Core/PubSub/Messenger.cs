#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Pooling;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
    public sealed class Messenger : IDisposable
    {
        private readonly SingletonVault<MessageBroker> _brokers = new();

#if UNITASK
        private readonly CappedArrayPool<Cysharp.Threading.Tasks.UniTask> _taskArrayPool;
#else
        private readonly CappedArrayPool<UnityEngine.Awaitable> _taskArrayPool;
#endif

        public Messenger()
        {
            _taskArrayPool = new(8);
            Subscriber = new(_brokers, _taskArrayPool);
            Publisher = new(_brokers, _taskArrayPool);
        }

        public MessageSubscriber Subscriber { get; }

        public MessagePublisher Publisher { get; }

        public void Dispose()
        {
            _brokers.Dispose();
        }
    }
}

#endif
