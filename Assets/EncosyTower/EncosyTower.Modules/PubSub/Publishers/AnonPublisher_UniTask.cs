#if UNITASK

using Cysharp.Threading.Tasks;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.PubSub.Internals;
using EncosyTower.Modules.Vaults;

namespace EncosyTower.Modules.PubSub
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
