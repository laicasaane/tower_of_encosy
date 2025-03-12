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
    public class MonoMultiPageList : MonoPageFlow
    {
        private MultiPageList<IMonoPage> _flow;

        protected override void OnInitialize(InitializationContext context)
        {
            _flow = new(Context);

            var subscriber = context.Subscriber.WithState(this);
            var subscriptions = context.Subscriptions;

            subscriber
                .Subscribe<AddPageAsyncMessage>(static (state, msg, _, tkn) => state.HandleAsync(msg, tkn))
                .AddTo(subscriptions);

            subscriber
                .Subscribe<ShowPageAtIndexAsyncMessage>(static (state, msg, _, tkn) => state.HandleAsync(msg, tkn))
                .AddTo(subscriptions);

            subscriber
                .Subscribe<HidePageAtIndexAsyncMessage>(static (state, msg, _, tkn) => state.HandleAsync(msg, tkn))
                .AddTo(subscriptions);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        private async UnityTask HandleAsync(AddPageAsyncMessage msg, CancellationToken token)
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

            var result = await _flow.AddAsync(identifier.Page, msg.Context, token);

            if (result == false)
            {
                ReturnPageToPool(identifier, msg.Context);
                return;
            }

            identifier.Transform.SetParent(RectTransform);
        }

        private async UnityTask HandleAsync(ShowPageAtIndexAsyncMessage msg, CancellationToken token)
        {
            var identifierOpt = GetPageIdentifierAt(msg.Index);

            if (identifierOpt.TryValue(out var identifier) == false)
            {
                return;
            }

            identifier.GameObject.SetActive(true);

            var result = await _flow.ShowAsync(msg.Index, msg.Context, token);

            if (token.IsCancellationRequested || result == false)
            {
                identifier.GameObject.SetActive(false);
            }
        }

        private async UnityTask HandleAsync(HidePageAtIndexAsyncMessage msg, CancellationToken token)
        {
            var identifierOpt = GetPageIdentifierAt(msg.Index);

            if (identifierOpt.TryValue(out var identifier) == false)
            {
                return;
            }

            var result = await _flow.HideAsync(msg.Index, msg.Context, token);

            if (token.IsCancellationRequested == false && result)
            {
                identifier.GameObject.SetActive(false);
            }
        }
    }
}

#endif
