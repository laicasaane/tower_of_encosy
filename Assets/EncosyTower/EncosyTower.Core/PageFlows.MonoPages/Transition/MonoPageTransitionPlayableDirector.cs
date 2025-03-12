#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine.Playables;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public class MonoPageTransitionPlayableDirector : MonoPageTransition
    {
        public PlayableDirector director;
        public Options onShowOptions;
        public Options onHideOptions;

        private bool _isTransitioning;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;

        public PlayableDirector Director
        {
            get
            {
                if (director.IsInvalid())
                {
                    director = this.GetOrAddComponent<PlayableDirector>();
                }

                return director;
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
            var director = Director;
            director.stopped += OnStopped;

            _isTransitioning = false;

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
            var director = Director;
            director.stopped -= OnStopped;
        }

        public override UnityTask OnShowAsync(PageTransitionOptions _, CancellationToken token)
        {
            return onShowOptions.playableAsset.IsInvalid()
                ? UnityTasks.GetCompleted()
                : Transition(onShowOptions, _zeroShowDuration, token);
        }

        public override UnityTask OnHideAsync(PageTransitionOptions _, CancellationToken token)
        {
            return onShowOptions.playableAsset.IsInvalid()
                ? UnityTasks.GetCompleted()
                : Transition(onHideOptions, _zeroHideDuration, token);
        }

        private async UnityTask Transition(Options options, bool zeroDuration, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var director = Director;
            director.Play(options.playableAsset, DirectorWrapMode.None);

            if (zeroDuration == false)
            {
                _isTransitioning = true;

                await UnityTasks.WaitUntil(this, static state => state._isTransitioning == false, token);

                if (token.IsCancellationRequested)
                {
                    _isTransitioning = false;
                }
            }
            else
            {
                director.time = options.playableAsset.duration;
            }
        }

        private void OnStopped(PlayableDirector _)
        {
            _isTransitioning = false;
        }

        [Serializable]
        public struct Options
        {
            public bool forceRun;
            public bool disableZeroDuration;
            public PlayableAsset playableAsset;
        }
    }
}

#endif
