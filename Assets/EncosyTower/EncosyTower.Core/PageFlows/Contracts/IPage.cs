#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public interface IPage { }

    public interface IPageCreateAsync : IPage
    {
        UnityTask OnCreateAsync(PageContext context, CancellationToken token);
    }

    public interface IPageTearDown : IPage
    {
        void OnTearDown(PageContext context);
    }

    public interface IPageAttachToFlowAsync : IPage
    {
        UnityTask OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }

    public interface IPageDetachFromFlowAsync : IPage
    {
        UnityTask OnDetachFromFlowAsync(IPageFlow flow, PageContext context, CancellationToken token);
    }
}

#endif
