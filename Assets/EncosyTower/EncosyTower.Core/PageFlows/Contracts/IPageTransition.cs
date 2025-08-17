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

    public interface IPageOnBeforeShowAsync : IPage
    {
        UnityTask OnBeforeShowAsync(PageContext context, CancellationToken token);
    }

    public interface IPageOnBeforeHideAsync : IPage
    {
        UnityTask OnBeforeHideAsync(PageContext context, CancellationToken token);
    }

    public interface IPageOnBeforeShow : IPage
    {
        void OnBeforeShow(PageContext context);
    }

    public interface IPageOnBeforeHide : IPage
    {
        void OnBeforeHide(PageContext context);
    }

    public interface IPageOnAfterShow : IPage
    {
        void OnAfterShow(PageContext context);
    }

    public interface IPageOnAfterHide : IPage
    {
        void OnAfterHide(PageContext context);
    }

    public interface IPageOnShowAsync : IPage
    {
        UnityTask OnShowAsync(PageContext context, CancellationToken token);
    }

    public interface IPageOnHideAsync : IPage
    {
        UnityTask OnHideAsync(PageContext context, CancellationToken token);
    }
}

#endif
