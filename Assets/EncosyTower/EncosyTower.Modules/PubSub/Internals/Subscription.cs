#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Modules.Collections;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed class Subscription<TMessage> : ISubscription
    {
        public static readonly Subscription<TMessage> None = new(default, default);

        private IHandler<TMessage> _handler;
        private readonly WeakReference<ArrayMap<DelegateId, IHandler<TMessage>>> _handlers;

        public Subscription(IHandler<TMessage> handler, ArrayMap<DelegateId, IHandler<TMessage>> handlers)
        {
            _handler = handler;
            _handlers = new WeakReference<ArrayMap<DelegateId, IHandler<TMessage>>>(handlers);
        }

        public void Dispose()
        {
            if (_handler == null)
            {
                return;
            }

            var id = _handler.Id;
            _handler.Dispose();
            _handler = null;

            if (_handlers.TryGetTarget(out var handlers))
            {
                handlers.Remove(id);
            }
        }
    }

    internal static class SubscriptionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTo<TMessage>(
              [NotNull] this Subscription<TMessage> subscription
            , CancellationToken unsubscribeToken
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
        {
            unsubscribeToken.Register(static x => ((Subscription<TMessage>)x)?.Dispose(), subscription);
        }
    }
}

#endif
