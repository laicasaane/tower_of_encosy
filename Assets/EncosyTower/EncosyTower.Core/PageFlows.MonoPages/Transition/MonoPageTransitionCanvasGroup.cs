#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

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
        public Options onShowOptions = Options.DefaultShow;
        public Options onHideOptions = Options.DefaultHide;

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

        public override bool ForceRunHide => onHideOptions.forceRun;

        public override bool ForceRunShow => onShowOptions.forceRun;

        public override UnityTask OnBeforeTransitionAsync(
              PageTransitionOperation operation
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        )
        {
            _isTransitioning = false;
            _elapsedTime = 0f;

            _zeroShowDuration = showOptions.Contains(PageTransitionOptions.ZeroDuration)
                && onShowOptions.disableZeroDuration == false;

            _zeroHideDuration = hideOptions.Contains(PageTransitionOptions.ZeroDuration)
                && onHideOptions.disableZeroDuration == false;

            return UnityTasks.GetCompleted();
        }

        public override void OnAfterTransition(
              PageTransitionOperation operation
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        )
        {
            var cg = CanvasGroup;
            cg.blocksRaycasts = onHideOptions.blockRaycast == BlockRaycastStrategy.Always
                || operation == PageTransitionOperation.Show;
        }

        public override UnityTask OnShowAsync(PageTransitionOptions _, CancellationToken token)
        {
            var cg = CanvasGroup;
            cg.blocksRaycasts = onShowOptions.blockRaycast == BlockRaycastStrategy.Always;

            return disableTweening
                ? UnityTasks.GetCompleted()
                : Transition(onShowOptions, _zeroShowDuration, token);
        }

        public override UnityTask OnHideAsync(PageTransitionOptions _, CancellationToken token)
        {
            return disableTweening
                ? UnityTasks.GetCompleted()
                : Transition(onHideOptions, _zeroHideDuration, token);
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
