#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public sealed class Messenger : IDisposable
    {
        private readonly SingletonVault<IMessageBroker> _messageBrokers = new();
        private readonly InterceptorBrokers _interceptorBrokers = new();

        public Messenger([NotNull] ArrayPool<UnityTask> taskArrayPool)
        {
            Subscriber = new(_messageBrokers, _interceptorBrokers, taskArrayPool);
            Publisher = new(_messageBrokers, _interceptorBrokers, taskArrayPool);
        }

        public MessageSubscriber Subscriber { get; }

        public MessagePublisher Publisher { get; }

        public MessageInterceptors Interceptors => new(_interceptorBrokers);

        public void Dispose()
        {
            _messageBrokers.Dispose();
            _interceptorBrokers.Dispose();
        }
    }
}

#endif
