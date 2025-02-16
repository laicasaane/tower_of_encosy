#if UNITASK

using Cysharp.Threading.Tasks;
using EncosyTower.Pooling;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
    partial class AnonPublisher
    {
        internal AnonPublisher(
              SingletonVault<MessageBroker> brokers
            , CappedArrayPool<UniTask> taskArrayPool
        )
        {
            _publisher = new(brokers, taskArrayPool);
        }
    }
}

#endif
