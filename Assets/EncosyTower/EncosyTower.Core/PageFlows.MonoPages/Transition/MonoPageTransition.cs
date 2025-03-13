#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public abstract class MonoPageTransition : MonoBehaviour, IPageTransition
    {
        public string identifier;

        public abstract bool ForceRunHide { get; }

        public abstract bool ForceRunShow { get; }

        public abstract UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        );

        public abstract void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        );

        public abstract UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token);

        public abstract UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token);
    }
}

#endif
