#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Common;
using EncosyTower.Pooling;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class InterceptablePublisherPool<TScope, TMessage> : IDisposable
#if !ENCOSY_PUBSUB_RELAX_MODE
        where TMessage : IMessage
#endif
    {
        private static readonly InterceptablePublisherPool<TScope, TMessage> s_instance;

        private readonly SimpleConcurrentPool<InterceptablePublisher<TScope, TMessage>> _pool;

        static InterceptablePublisherPool()
        {
            s_instance = new InterceptablePublisherPool<TScope, TMessage>();
            AutoDisposeManager.Register(s_instance);
        }

        private InterceptablePublisherPool()
        {
            _pool = new(
                  createFunc: static () => new()
                , actionOnRent: static x => x.Initialize()
                , actionOnReturn: static x => x.Deinitialize()
            );
        }

        public static InterceptablePublisher<TScope, TMessage> Rent()
        {
            return s_instance._pool.Rent();
        }

        public static void Return(InterceptablePublisher<TScope, TMessage> item)
        {
            s_instance._pool.Return(item);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}

#endif
