#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Common;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    public interface IPage { }

    public interface IPageOnCreateAsync : IPage
    {
        UnityTaskBool OnCreateAsync(PageContext context, CancellationToken token);
    }

    public interface IPageOnReturnToPool : IPage
    {
        void OnReturnToPool(PageContext context);
    }

    public interface IPageOnAttachToFlowAsync : IPage
    {
        UnityTaskBool OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }

    public interface IPageOnDetachFromFlowAsync : IPage
    {
        UnityTaskBool OnDetachFromFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }

    public interface IPageHasOptions : IPage
    {
        PageOptions PageOptions { get; }
    }

    public interface IPageHasTransition : IPage
    {
        IPageTransition PageTransition { get; }
    }

    public interface IPageNeedsFlowId : IPage
    {
        long FlowId { set; }
    }

    public interface IPageNeedsMessageSubscriber : IPage
    {
        MessageSubscriber Subscriber { set; }
    }

    public interface IPageNeedsMessagePublisher : IPage
    {
        MessagePublisher Publisher { set; }
    }

    public interface IPageNeedsFlowScopes<TFlowScopes> : IPage
        where TFlowScopes : struct, IPageFlowScopeCollection
    {
        Option<TFlowScopes> FlowScopes { set; }
    }
}

#endif
