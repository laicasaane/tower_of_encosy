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
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        );

        void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        );

        UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token);

        UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token);
    }

    public interface IPageBeforeTransitionAsync : IPage
    {
        UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageContext context
            , CancellationToken token
        );
    }

    public interface IPageAfterTransition : IPage
    {
        void OnAfterTransition(PageTransition transition, PageContext context);
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
