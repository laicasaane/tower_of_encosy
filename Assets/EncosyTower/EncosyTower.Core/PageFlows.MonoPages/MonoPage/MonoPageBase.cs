#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public abstract class MonoPageBase : MonoBehaviour
        , IMonoPage
        , IPageHasFlowId
        , IPageHasOptions
        , IPageHasTransition
    {
        private MonoPageOptions _options;
        private MonoPageTransitionCollection _transition;

        public long FlowId { get; set; }

        public PageOptions PageOptions
        {
            get
            {
                if (_options.IsInvalid())
                {
                    _options = this.GetOrAddComponent<MonoPageOptions>();
                }

                return _options.PageOptions;
            }
        }

        public IPageTransition PageTransition
        {
            get
            {
                if (_transition.IsInvalid())
                {
                    _transition = this.GetOrAddComponent<MonoPageTransitionCollection>();
                }

                return _transition;
            }
        }
    }
}

#endif
