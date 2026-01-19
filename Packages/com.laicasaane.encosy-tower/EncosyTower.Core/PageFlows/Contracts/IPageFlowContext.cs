#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Buffers;
using EncosyTower.Common;
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

        MessageSubscriber Subscriber { get; }

        MessagePublisher Publisher { get; }

        PageFlowScope FlowScope { get; }

        Option<IPageFlowScopeCollectionApplier> FlowScopeCollectionApplier { get; }

        bool WarnNoSubscriber { get; }

        ILogger Logger { get; }
    }
}

#endif
