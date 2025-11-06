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
    public class MonoSinglePageList : MonoPageFlow
    {
        private SinglePageList<IMonoPage> _flow;

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

        public async UnityTask HideActivePageAsync(PageContext context, CancellationToken token)
        {
            var pageOpt = _flow.CurrentPage;

            if (pageOpt.TryGetValue(out var page) == false)
            {
                return;
            }

            var result = await _flow.HideAsync(context, token);

            if (token.IsCancellationRequested || result == false)
            {
                return;
            }

            var index = _flow.IndexOf(page);

            if (index < 0)
            {
                return;
            }

            var identifierOpt = GetPageIdentifierAt(index);

            if (identifierOpt.TryGetValue(out var identifier))
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
            subscriber.Subscribe<HideActivePageAsyncMessage>(HandleAsync);

            var processHub = context.ProcessHub.WithState(this);
            processHub.Register<IsInTransitionRequest, bool>(Process);
            processHub.Register<GetPageIndexRequest, int>(Process);
            processHub.Register<GetCurrentPageRequest, Option<IMonoPage>>(Process);
            processHub.Register<GetPageListRequest, IReadOnlyList<IMonoPage>>(Process);
            processHub.Register<GetPageCollectionRequest, IReadOnlyCollection<IMonoPage>>(Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoSinglePageList list
            , AddPageAsyncMessage msg
            , CancellationToken token
        )
        {
            return list.AddPageAsync(msg.AssetKey, msg.Context, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoSinglePageList list
            , ShowPageAtIndexAsyncMessage msg
            , CancellationToken token
        )
        {
            return list.ShowPageAtIndexAsync(msg.Index, msg.Context, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoSinglePageList list
            , HideActivePageAsyncMessage msg
            , CancellationToken token
        )
        {
            return list.HideActivePageAsync(msg.Context, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(MonoSinglePageList list, IsInTransitionRequest _)
            => list._flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Process(MonoSinglePageList list, GetPageIndexRequest req)
            => list._flow.IndexOf(req.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IMonoPage> Process(MonoSinglePageList list, GetCurrentPageRequest _)
            => list._flow.CurrentPage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyList<IMonoPage> Process(MonoSinglePageList list, GetPageListRequest _)
            => list._flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IMonoPage> Process(MonoSinglePageList list, GetPageCollectionRequest _)
            => list._flow.Pages;
    }
}

#endif
