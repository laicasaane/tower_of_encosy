#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public partial class MessagePublisher
    {
        private readonly SingletonVault<IMessageBroker> _messageBrokers;
        private readonly ArrayPool<UnityTask> _taskArrayPool;

        internal MessagePublisher(
              SingletonVault<IMessageBroker> messageBrokers
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            _messageBrokers = messageBrokers;
            _taskArrayPool = taskArrayPool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Publisher<GlobalScope> Global()
        {
            return new(this, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Publisher<TScope> Scope<TScope>()
            where TScope : struct
        {
            return new(this, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Publisher<TScope> Scope<TScope>([NotNull] TScope scope)
        {
            return new(this, scope);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CachedPublisher<TMessage> GlobalCache<TMessage>(ILogger logger = null)
#if ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : new()
#else
            where TMessage : IMessage, new()
#endif
        {
            return Global().Cache<TMessage>(logger);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CachedPublisher<TMessage> Cache<TScope, TMessage>(ILogger logger = null)
            where TScope : struct
#if ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : new()
#else
            where TMessage : IMessage, new()
#endif
        {
            return Scope(default(TScope)).Cache<TMessage>(logger);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CachedPublisher<TMessage> Cache<TScope, TMessage>([NotNull] TScope scope, ILogger logger = null)
#if ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : new()
#else
            where TMessage : IMessage, new()
#endif
        {
            return Scope(scope).Cache<TMessage>(logger);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityPublisher<TScope> UnityScope<TScope>([NotNull] TScope scope)
            where TScope : UnityEngine.Object
        {
            return new(this, scope);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CachedPublisher<TMessage> UnityCache<TScope, TMessage>([NotNull] TScope scope, ILogger logger = null)
            where TScope : UnityEngine.Object
#if ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : new()
#else
            where TMessage : IMessage, new()
#endif
        {
            return UnityScope(scope).Cache<TMessage>(logger);
        }
    }
}

#endif
