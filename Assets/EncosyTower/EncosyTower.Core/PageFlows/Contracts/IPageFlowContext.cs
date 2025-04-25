#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Buffers;
using EncosyTower.Logging;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public interface IPageFlowContext
    {
        ArrayPool<UnityTask> TaskArrayPool { get; }

        MessageSubscriber.Subscriber<PageFlowScope> Subscriber { get; }

        MessagePublisher.Publisher<PageFlowScope> Publisher { get; }

        bool SlimPublishingContext { get; }

        bool IgnoreEmptySubscriber { get; }

        ILogger Logger { get; }
    }
}

#endif
