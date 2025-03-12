#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public interface IPageTransition
    {
        bool ForceRunHide { get; }

        bool ForceRunShow { get; }

        UnityTask OnBeforeTransitionAsync(
              PageTransitionOperation operation
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        );

        void OnAfterTransition(
              PageTransitionOperation operation
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        );

        UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token);

        UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token);
    }

    public interface IPageBeforeTransitionAsync : IPage
    {
        UnityTask OnBeforeTransitionAsync(
              PageTransitionOperation operation
            , PageContext context
            , CancellationToken token
        );
    }

    public interface IPageAfterTransition : IPage
    {
        void OnAfterTransition(PageTransitionOperation operation, PageContext context);
    }

    public interface IPageShowAsync : IPage
    {
        UnityTask OnShowAsync(PageContext context, CancellationToken token);
    }

    public interface IPageHideAsync : IPage
    {
        UnityTask OnHideAsync(PageContext context, CancellationToken token);
    }
}

#endif
