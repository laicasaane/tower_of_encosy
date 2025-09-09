#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public class MonoPageTransitionGraphic : MonoPageTransition
    {
        public Graphic graphic;
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

        public Graphic Graphic
        {
            get
            {
                if (graphic.IsInvalid())
                {
                    graphic = this.GetOrAddComponent<Image>();
                }

                return graphic;
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
            _ = Graphic;
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
        }

        public override UnityTask OnShowAsync(PageTransitionOptions _, CancellationToken token)
        {
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

            var graphic = Graphic;
            var alpha = options.alpha;
            _alpha = new(Mathf.Clamp(alpha.start, 0f, 1f), Mathf.Clamp(alpha.end, 0f, 1f));
            _duration = Mathf.Max(0f, options.duration);

            if (_duration > 0f && zeroDuration == false)
            {
                _isTransitioning = true;

                var color = graphic.color;
                color.a = Mathf.Clamp(alpha.start, 0f, 1f);
                graphic.color = color;

                await UnityTasks.WaitUntil(this, static state => state._isTransitioning == false, token);

                if (token.IsCancellationRequested)
                {
                    _isTransitioning = false;
                }
            }
            else
            {
                var color = graphic.color;
                color.a = Mathf.Clamp(alpha.end, 0f, 1f);
                graphic.color = color;
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
            var progress = Mathf.Clamp(elapsedTime / duration, 0f, 1f);
            var alpha = _alpha;
            var color = graphic.color;
            color.a = Mathf.Lerp(alpha.start, alpha.end, progress);

            graphic.color = color;

            if (elapsedTime >= duration)
            {
                _isTransitioning = false;
            }
        }

        [Serializable]
        public struct Options
        {
            public bool forceRun;
            public bool disableZeroDuration;
            public float duration;
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
