#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.PubSub;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public abstract class MonoPageBase : MonoBehaviour
        , IMonoPage
        , IPageHasOptions
        , IPageHasTransition
        , IPageNeedsFlowId
        , IPageNeedsMessageSubscriber
        , IPageNeedsMessagePublisher
    {
        private MonoPageOptions _options;
        private MonoPageTransitionCollection _transition;

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

        public long FlowId { get; set; }

        public MessageSubscriber.Subscriber<PageFlowScope> Subscriber { get; set; }

        public MessagePublisher.Publisher<PageFlowScope> Publisher { get; set; }
    }
}

#endif
