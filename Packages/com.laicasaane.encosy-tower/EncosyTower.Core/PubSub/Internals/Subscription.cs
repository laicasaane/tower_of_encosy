#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class Subscription<TMessage> : ISubscription
    {
        public static readonly Subscription<TMessage> None = new(default, default, 0);

        private readonly WeakReference<MessageBroker<TMessage>> _broker;
        private readonly int _order;
        private IHandler<TMessage> _handler;

        public Subscription([NotNull] MessageBroker<TMessage> broker, [NotNull] IHandler<TMessage> handler, int order)
        {
            _broker = new WeakReference<MessageBroker<TMessage>>(broker);
            _handler = handler;
            _order = order;
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

            if (_broker.TryGetTarget(out var broker))
            {
                broker.RemoveHandler(id, _order);
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
