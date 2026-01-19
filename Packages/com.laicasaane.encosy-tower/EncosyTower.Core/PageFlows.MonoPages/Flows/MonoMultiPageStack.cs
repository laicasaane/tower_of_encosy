#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Processing;
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

            if (identifierOpt.TryGetValue(out var identifier) == false)
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

            if (pageOpt.TryGetValue(out var page) == false)
            {
                return;
            }

            var result = await _flow.PopAsync(context, token);

            if (token.IsCancellationRequested == false && result)
            {
                ReturnPageToPool(page, context);
            }
        }

        protected override void OnInitialize(in InitializationContext context)
        {
            _flow = new(Context);

            var subscriber = context.Subscriber.WithState(this);
            subscriber.Subscribe<ShowPageAsyncMessage>(HandleAsync);
            subscriber.Subscribe<HideActivePageAsyncMessage>(HandleAsync);

            var processHub = context.ProcessHub.WithState(this);
            processHub.Register<IsInTransitionRequest, bool>(Process);
            processHub.Register<GetCurrentPageRequest, Option<IMonoPage>>(Process);
            processHub.Register<GetPageCollectionRequest, IReadOnlyCollection<IMonoPage>>(Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoMultiPageStack stack
            , ShowPageAsyncMessage msg
            , PublishingContext context
        )
        {
            return stack.ShowPageAsync(msg.AssetKey, msg.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoMultiPageStack stack
            , HideActivePageAsyncMessage msg
            , PublishingContext context
        )
        {
            return stack.HideActivePageAsync(msg.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(MonoMultiPageStack stack, IsInTransitionRequest _)
            => stack._flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IMonoPage> Process(MonoMultiPageStack stack, GetCurrentPageRequest _)
            => stack._flow.CurrentPage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IMonoPage> Process(MonoMultiPageStack stack, GetPageCollectionRequest _)
            => stack._flow.Pages;
    }
}

#endif
