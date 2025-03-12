#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Collections;
using EncosyTower.PubSub;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class MonoSinglePageStack : MonoPageFlow
    {
        private SinglePageStack<IMonoPage> _flow;

        protected override void OnInitialize(InitializationContext context)
        {
            _flow = new(Context);

            var subscriber = context.Subscriber.WithState(this);
            var subscriptions = context.Subscriptions;

            subscriber
                .Subscribe<ShowPageAsyncMessage>(static (state, msg, _, tkn) => state.HandleAsync(msg, tkn))
                .AddTo(subscriptions);

            subscriber
                .Subscribe<HideActivePageAsyncMessage>(static (state, msg, _, tkn) => state.HandleAsync(msg, tkn))
                .AddTo(subscriptions);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        private async UnityTask HandleAsync(ShowPageAsyncMessage msg, CancellationToken token)
        {
            var pageKey = MakePageKey(msg.AssetKey);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var identifierOpt = await RentPageAsync(pageKey, msg.Context, token);

            if (identifierOpt.TryValue(out var identifier) == false)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                ReturnPageToPool(identifier, msg.Context);
                return;
            }

            identifier.GameObject.SetActive(true);
            identifier.Transform.SetParent(RectTransform);

            var oldPageOpt = _flow.CurrentPage;
            var result = await _flow.PushAsync(identifier.Page, msg.Context, token);

            if (token.IsCancellationRequested || result == false)
            {
                ReturnPageToPool(identifier, msg.Context);
                return;
            }

            if (oldPageOpt.TryValue(out var oldPage))
            {
                ReturnPageToPool(oldPage, msg.Context);
            }
        }

        private async UnityTask HandleAsync(HideActivePageAsyncMessage msg, CancellationToken token)
        {
            var pageOpt = _flow.CurrentPage;

            if (pageOpt.TryValue(out var page) == false)
            {
                return;
            }

            var result = await _flow.PopAsync(msg.Context, token);

            if (token.IsCancellationRequested == false && result)
            {
                ReturnPageToPool(page, msg.Context);
            }
        }
    }
}

#endif
