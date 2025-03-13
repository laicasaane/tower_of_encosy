#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    [Serializable]
    public struct PageOptions
    {
        public ShowOperationOptions showOptions;
        public HideOperationOptions hideOptions;

        public static PageOptions.Options SelectHideOptions(
              PageTransition transition
            , in PageOptions pageToHideOptions
            , in PageOptions pageToShowOptions
        )
        {
            return transition == PageTransition.Hide
                ? pageToHideOptions.hideOptions.hideThisPage
                : pageToShowOptions.showOptions.hideOtherPage;
        }

        public static PageOptions.Options SelectShowOptions(
              PageTransition transition
            , in PageOptions pageToHideOptions
            , in PageOptions pageToShowOptions
        )
        {
            return transition == PageTransition.Hide
                ? pageToHideOptions.hideOptions.showOtherPage
                : pageToShowOptions.showOptions.showThisPage;
        }

        [Serializable]
        public struct ShowOperationOptions
        {
            public Options showThisPage;
            public Options hideOtherPage;
        }

        [Serializable]
        public struct HideOperationOptions
        {
            public Options hideThisPage;
            public Options showOtherPage;
        }

        [Serializable]
        public struct Options
        {
            public bool forceUse;
            public PageTransitionOptions transitionOptions;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly Option<PageTransitionOptions> GetTransitionOptions()
                => forceUse ? transitionOptions : default(Option<PageTransitionOptions>);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetTransitionOptions(Option<PageTransitionOptions> value)
                => transitionOptions = value.ValueOrDefault(transitionOptions);
        }
    }
}

#endif
