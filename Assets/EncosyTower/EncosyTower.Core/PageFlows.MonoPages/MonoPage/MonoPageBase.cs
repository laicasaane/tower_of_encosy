#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
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
        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private long _flowId;

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

        public long FlowId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flowId;
        }

        public MessageSubscriber Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subscriber;
        }

        public MessagePublisher Publisher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _publisher;
        }

        long IPageNeedsFlowId.FlowId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _flowId = value;
        }

        MessageSubscriber IPageNeedsMessageSubscriber.Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _subscriber = value;
        }

        MessagePublisher IPageNeedsMessagePublisher.Publisher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _publisher = value;
        }
    }
}

#endif
