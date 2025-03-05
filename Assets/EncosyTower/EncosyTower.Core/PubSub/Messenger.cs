#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
    public sealed class Messenger : IDisposable
    {
        private readonly SingletonVault<MessageBroker> _brokers = new();

        public Messenger([NotNull] ArrayPool<UniTask> taskArrayPool)
        {
            Subscriber = new(_brokers, taskArrayPool);
            Publisher = new(_brokers, taskArrayPool);
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
