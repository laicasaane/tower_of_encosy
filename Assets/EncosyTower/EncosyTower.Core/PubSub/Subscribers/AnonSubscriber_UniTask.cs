#if UNITASK

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using Cysharp.Threading.Tasks;
using EncosyTower.Pooling;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
    partial class AnonSubscriber
    {
        internal AnonSubscriber(
              SingletonVault<MessageBroker> brokers
            , CappedArrayPool<UniTask> taskArrayPool
        )
        {
            _subscriber = new(brokers, taskArrayPool);
        }
    }
}

#endif
