#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public class MonoPageTransitionCanvasGroup : MonoPageTransition
    {
        public CanvasGroup canvasGroup;
        public bool disableTweening;

        [FormerlySerializedAs("onShowOptions")]
        public Options showOperationOptions = Options.DefaultShow;

        [FormerlySerializedAs("onHideOptions")]
        public Options hideOperationOptions = Options.DefaultHide;

        private bool _isTransitioning;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;
        private float _elapsedTime;
        private float _duration;
        private TransitionFloat _alpha;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup.IsInvalid())
                {
                    canvasGroup = this.GetOrAddComponent<CanvasGroup>();
                }

                return canvasGroup;
            }
        }

        public override bool ForceRunHide => hideOperationOptions.forceRun;

        public override bool ForceRunShow => showOperationOptions.forceRun;

        public override UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        )
        {
            _isTransitioning = false;
            _elapsedTime = 0f;

            _zeroShowDuration = showOptions.Contains(PageTransitionOptions.ZeroDuration)
                && showOperationOptions.disableZeroDuration == false;

            _zeroHideDuration = hideOptions.Contains(PageTransitionOptions.ZeroDuration)
                && hideOperationOptions.disableZeroDuration == false;

            return UnityTasks.GetCompleted();
        }

        public override void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        )
        {
            var cg = CanvasGroup;
            cg.blocksRaycasts = hideOperationOptions.blockRaycast == BlockRaycastStrategy.Always
                || transition == PageTransition.Show;
        }

        public override UnityTask OnShowAsync(PageTransitionOptions _, CancellationToken token)
        {
            var cg = CanvasGroup;
            cg.blocksRaycasts = showOperationOptions.blockRaycast == BlockRaycastStrategy.Always;

            return disableTweening
                ? UnityTasks.GetCompleted()
                : Transition(showOperationOptions, _zeroShowDuration, token);
        }

        public override UnityTask OnHideAsync(PageTransitionOptions _, CancellationToken token)
        {
            return disableTweening
                ? UnityTasks.GetCompleted()
                : Transition(hideOperationOptions, _zeroHideDuration, token);
        }

        private async UnityTask Transition(Options options, bool zeroDuration, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var alpha = options.alpha;
            _alpha = new(Mathf.Clamp(alpha.start, 0f, 1f), Mathf.Clamp(alpha.end, 0f, 1f));
            _duration = Mathf.Max(0f, options.duration);

            var cg = CanvasGroup;

            if (_duration > 0f && zeroDuration == false)
            {
                _isTransitioning = true;

                cg.alpha = Mathf.Clamp(alpha.start, 0f, 1f);

                await UnityTasks.WaitUntil(this, static state => state._isTransitioning == false, token);

                if (token.IsCancellationRequested)
                {
                    _isTransitioning = false;
                }
            }
            else
            {
                cg.alpha = Mathf.Clamp(alpha.end, 0f, 1f);
            }
        }

        private void Update()
        {
            if (disableTweening || _isTransitioning == false)
            {
                return;
            }

            var elapsedTime = _elapsedTime += Time.smoothDeltaTime;

            var duration = _duration;
            var alpha = _alpha;
            var progress = Mathf.Clamp(elapsedTime / duration, 0f, 1f);
            canvasGroup.alpha = Mathf.Lerp(alpha.start, alpha.end, progress);

            if (elapsedTime >= duration)
            {
                _isTransitioning = false;
            }
        }

        public enum BlockRaycastStrategy
        {
            Auto,
            Always,
        }

        [Serializable]
        public struct Options
        {
            public bool forceRun;
            public bool disableZeroDuration;
            public float duration;
            public BlockRaycastStrategy blockRaycast;
            public TransitionFloat alpha;

            public static Options DefaultShow => new() {
                duration = 0.2f,
                alpha = new(0f, 1f),
            };

            public static Options DefaultHide => new() {
                duration = 0.2f,
                alpha = new(1f, 0f),
            };
        }
    }
}

#endif
