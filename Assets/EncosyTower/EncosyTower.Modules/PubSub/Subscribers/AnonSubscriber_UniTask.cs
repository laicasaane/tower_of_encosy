#if UNITASK

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PUBSUB_NO_VALIDATION__
#else
#define __MODULE_CORE_PUBSUB_VALIDATION__
#endif

using Cysharp.Threading.Tasks;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.PubSub.Internals;
using EncosyTower.Modules.Vaults;

namespace EncosyTower.Modules.PubSub
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
