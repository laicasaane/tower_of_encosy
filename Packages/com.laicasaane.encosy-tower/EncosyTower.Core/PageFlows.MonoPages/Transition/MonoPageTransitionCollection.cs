#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    [DisallowMultipleComponent]
    public sealed class MonoPageTransitionCollection : MonoBehaviour, IPageTransition
    {
        public bool alwaysEnableEnter = true;
        public bool alwaysEnableExit = true;
        public ChildrenOptions childrenOptions = ChildrenOptions.Default;

        private MonoPageTransition[] _transitions;
        private UnityTask[] _tasks;

        public bool ForceRunHide => alwaysEnableExit;

        public bool ForceRunShow => alwaysEnableEnter;

        private void Awake()
        {
            if (childrenOptions.include)
            {
                _transitions = GetComponentsInChildren<MonoPageTransition>(childrenOptions.includeInactive);
            }
            else
            {
                _transitions = GetComponents<MonoPageTransition>();
            }

            _tasks = new UnityTask[_transitions.Length];
        }

        public UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
         )
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;
            var tasks = _tasks.AsSpan();

            for (var i = 0; i < length; i++)
            {
                tasks[i] = transitions[i].OnBeforeTransitionAsync(transition, showOptions, hideOptions, token);
            }

            return UnityTasks.WhenAll(_tasks);
        }

        public void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        )
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;

            for (var i = 0; i < length; i++)
            {
                transitions[i].OnAfterTransition(transition, showOptions, hideOptions);
            }
        }

        public UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token)
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;
            var tasks = _tasks.AsSpan();
            var withShow = options.Contains(PageTransitionOptions.NoTransition) == false;

            for (var i = 0; i < length; i++)
            {
                var transition = transitions[i];

                tasks[i] = withShow || transition.ForceRunShow
                    ? transition.OnShowAsync(options, token)
                    : UnityTasks.GetCompleted();
            }

            return UnityTasks.WhenAll(_tasks);
        }

        public UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token)
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;
            var tasks = _tasks.AsSpan();
            var withHide = options.Contains(PageTransitionOptions.NoTransition) == false;

            for (var i = 0; i < length; i++)
            {
                var transition = transitions[i];

                tasks[i] = withHide || transition.ForceRunHide
                    ? transition.OnHideAsync(options, token)
                    : UnityTasks.GetCompleted();
            }

            return UnityTasks.WhenAll(_tasks);
        }

        [Serializable]
        public struct ChildrenOptions
        {
            public bool include;
            public bool includeInactive;

            public static ChildrenOptions Default => new() {
                include = true,
            };
        }
    }
}

#endif
