#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
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
    public class MonoMultiPageStack : MonoPageFlow
    {
        private MultiPageStack<IMonoPage> _flow;

        public async UnityTask ShowPageAsync(string assetKey, PageContext context, CancellationToken token)
        {
            var pageKey = MakePageKey(assetKey);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var identifierOpt = await RentPageAsync(pageKey, context, token);

            if (identifierOpt.TryValue(out var identifier) == false)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                ReturnPageToPool(identifier, context);
                return;
            }

            identifier.GameObject.SetActive(true);
            identifier.Transform.SetParent(RectTransform);

            var result = await _flow.PushAsync(identifier.Page, context, token);

            if (token.IsCancellationRequested || result == false)
            {
                ReturnPageToPool(identifier, context);
            }
        }
        public async UnityTask HideActivePageAsync(PageContext context, CancellationToken token)
        {
            var pageOpt = _flow.CurrentPage;

            if (pageOpt.TryValue(out var page) == false)
            {
                return;
            }

            var result = await _flow.PopAsync(context, token);

            if (token.IsCancellationRequested == false && result)
            {
                ReturnPageToPool(page, context);
            }
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityTask HandleAsync(ShowPageAsyncMessage msg, CancellationToken token)
            => ShowPageAsync(msg.AssetKey, msg.Context, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityTask HandleAsync(HideActivePageAsyncMessage msg, CancellationToken token)
            => HideActivePageAsync(msg.Context, token);
    }
}

#endif
