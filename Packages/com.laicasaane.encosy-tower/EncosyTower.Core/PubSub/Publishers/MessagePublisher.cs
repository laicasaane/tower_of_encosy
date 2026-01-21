#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using EncosyTower.UnityExtensions;
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
        private readonly InterceptorBrokers _interceptorBrokers;
        private readonly ArrayPool<UnityTask> _taskArrayPool;

        internal MessagePublisher(
              SingletonVault<IMessageBroker> messageBrokers
            , InterceptorBrokers interceptorBrokers
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            _messageBrokers = messageBrokers;
            _interceptorBrokers = interceptorBrokers;
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
        public UnityPublisher<TScope> UnityScope<TScope>([NotNull] TScope scope)
            where TScope : UnityEngine.Object
        {
            return new(this, scope);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CachedPublisher<GlobalScope, TMessage> GlobalCache<TMessage>(
              [NotNull] Func<TMessage> createFunc
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            return Global().Cache<TMessage>(createFunc, logger);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CachedPublisher<TScope, TMessage> Cache<TScope, TMessage>(
              [NotNull] Func<TMessage> createFunc
            , ILogger logger = null
        )
            where TScope : struct
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            return Scope(default(TScope)).Cache<TMessage>(createFunc, logger);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CachedPublisher<TScope, TMessage> Cache<TScope, TMessage>(
              [NotNull] Func<TMessage> createFunc
            , [NotNull] TScope scope
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            return Scope(scope).Cache<TMessage>(createFunc, logger);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if UNITY_6000_2_OR_NEWER
                CachedPublisher<UnityEntityId<TScope>, TMessage>
#else
                CachedPublisher<UnityInstanceId<TScope>, TMessage>
#endif
        UnityCache<TScope, TMessage>(
              [NotNull] Func<TMessage> createFunc
            , [NotNull] TScope scope
            , ILogger logger = null
       )
            where TScope : UnityEngine.Object
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            return UnityScope(scope).Cache<TMessage>(createFunc, logger);
        }
    }
}

#endif
