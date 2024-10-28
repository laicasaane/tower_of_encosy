#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.PubSub.Internals;
using EncosyTower.Modules.Vaults;

namespace EncosyTower.Modules.PubSub
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
            MessageSubscriber = new(_brokers, _taskArrayPool);
            MessagePublisher = new(_brokers, _taskArrayPool);
            AnonSubscriber = new(_brokers, _taskArrayPool);
            AnonPublisher = new(_brokers, _taskArrayPool);
        }

        public MessageSubscriber MessageSubscriber { get; }

        public MessagePublisher MessagePublisher { get; }

        public AnonSubscriber AnonSubscriber { get; }

        public AnonPublisher AnonPublisher { get; }

        public void Dispose()
        {
            _brokers.Dispose();
        }
    }
}

#endif
