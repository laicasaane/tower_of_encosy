#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
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
    public class MonoMultiPageList : MonoPageFlow
    {
        private MultiPageList<IMonoPage> _flow;

        public async UnityTask AddPageAsync(string assetKey, PageContext context, CancellationToken token)
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

            var result = await _flow.AddAsync(identifier.Page, context, token);

            if (result == false)
            {
                ReturnPageToPool(identifier, context);
                return;
            }

            identifier.Transform.SetParent(RectTransform);
        }

        public async UnityTask ShowPageAtIndexAsync(int index, PageContext context, CancellationToken token)
        {
            var identifierOpt = GetPageIdentifierAt(index);

            if (identifierOpt.TryGetValue(out var identifier) == false)
            {
                return;
            }

            identifier.GameObject.SetActive(true);

            var result = await _flow.ShowAsync(index, context, token);

            if (token.IsCancellationRequested || result == false)
            {
                identifier.GameObject.SetActive(false);
            }
        }

        public async UnityTask HidePageAtIndexAsync(int index, PageContext context, CancellationToken token)
        {
            var identifierOpt = GetPageIdentifierAt(index);

            if (identifierOpt.TryGetValue(out var identifier) == false)
            {
                return;
            }

            var result = await _flow.HideAsync(index, context, token);

            if (token.IsCancellationRequested == false && result)
            {
                identifier.GameObject.SetActive(false);
            }
        }

        protected override void OnInitialize(in InitializationContext context)
        {
            _flow = new(Context);

            var subscriber = context.Subscriber.WithState(this);
            subscriber.Subscribe<AddPageAsyncMessage>(HandleAsync);
            subscriber.Subscribe<ShowPageAtIndexAsyncMessage>(HandleAsync);
            subscriber.Subscribe<HidePageAtIndexAsyncMessage>(HandleAsync);

            var processHub = context.ProcessHub.WithState(this);
            processHub.Register<IsInTransitionRequest, bool>(Process);
            processHub.Register<GetPageIndexRequest, int>(Process);
            processHub.Register<GetPageListRequest, IReadOnlyList<IMonoPage>>(Process);
            processHub.Register<GetPageCollectionRequest, IReadOnlyCollection<IMonoPage>>(Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoMultiPageList list
            , AddPageAsyncMessage msg
            , PublishingContext context
        )
        {
            return list.AddPageAsync(msg.AssetKey, msg.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoMultiPageList list
            , ShowPageAtIndexAsyncMessage msg
            , PublishingContext context
        )
        {
            return list.ShowPageAtIndexAsync(msg.Index, msg.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoMultiPageList list
            , HidePageAtIndexAsyncMessage msg
            , PublishingContext context
        )
        {
            return list.HidePageAtIndexAsync(msg.Index, msg.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(MonoMultiPageList list, IsInTransitionRequest _)
            => list._flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Process(MonoMultiPageList list, GetPageIndexRequest req)
            => list._flow.IndexOf(req.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyList<IMonoPage> Process(MonoMultiPageList list, GetPageListRequest _)
            => list._flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IMonoPage> Process(MonoMultiPageList list, GetPageCollectionRequest _)
            => list._flow.Pages;
    }
}

#endif
