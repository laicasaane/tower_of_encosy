#if UNITASK

using Cysharp.Threading.Tasks;
using Module.Core.Collections;
using Module.Core.PubSub.Internals;
using Module.Core.Vaults;

namespace Module.Core.PubSub
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
