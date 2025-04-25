#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections;
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

            if (identifierOpt.TryValue(out var identifier) == false)
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

            if (identifierOpt.TryValue(out var identifier) == false)
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

            if (pageOpt.TryValue(out var page) == false)
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

            if (identifierOpt.TryValue(out var identifier))
            {
                identifier.GameObject.SetActive(false);
            }
        }

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
                .Subscribe<HideActivePageAsyncMessage>(static (state, msg, _, tkn) => state.HandleAsync(msg, tkn))
                .AddTo(subscriptions);

            var processHub = context.ProcessHub.WithState(this);
            var processRegistries = context.ProcessRegistries;

            processHub
                .Register<IsInTransitionRequest, bool>(static (state, proc) => state.Process(proc))
                .AddTo(processRegistries);

            processHub
                .Register<GetPageIndexRequest, int>(static (state, proc) => state.Process(proc))
                .AddTo(processRegistries);

            processHub
                .Register<GetCurrentPageRequest, Option<IMonoPage>>(static (state, proc) => state.Process(proc))
                .AddTo(processRegistries);

            processHub
                .Register<GetPageListRequest, IReadOnlyList<IMonoPage>>(static (state, proc) => state.Process(proc))
                .AddTo(processRegistries);

            processHub
                .Register<GetPageCollectionRequest, IReadOnlyCollection<IMonoPage>>(static (state, proc) => state.Process(proc))
                .AddTo(processRegistries);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityTask HandleAsync(AddPageAsyncMessage msg, CancellationToken token)
            => AddPageAsync(msg.AssetKey, msg.Context, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityTask HandleAsync(ShowPageAtIndexAsyncMessage msg, CancellationToken token)
            => ShowPageAtIndexAsync(msg.Index, msg.Context, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityTask HandleAsync(HideActivePageAsyncMessage msg, CancellationToken token)
            => HideActivePageAsync(msg.Context, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Process(IsInTransitionRequest _)
            => _flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Process(GetPageIndexRequest req)
            => _flow.IndexOf(req.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Option<IMonoPage> Process(GetCurrentPageRequest _)
            => _flow.CurrentPage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IReadOnlyList<IMonoPage> Process(GetPageListRequest _)
            => _flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IReadOnlyCollection<IMonoPage> Process(GetPageCollectionRequest _)
            => _flow.Pages;
    }
}

#endif
