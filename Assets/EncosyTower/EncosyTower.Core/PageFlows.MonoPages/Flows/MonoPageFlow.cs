#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.StringIds;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
    using UnityTaskPageOpt = Cysharp.Threading.Tasks.UniTask<Option<MonoPageIdentifier>>;
#else
    using UnityTask = UnityEngine.Awaitable;
    using UnityTaskPageOpt = UnityEngine.Awaitable<Option<MonoPageIdentifier>>;
#endif

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class MonoPageFlow : MonoBehaviour
    {
        [SerializeField] internal MonoPageFlowContext _context = new();

        private readonly Dictionary<StringId, MonoPagePool> _pageIdToPool = new();
        private readonly FasterList<MonoPageIdentifier> _pageIds = new();
        private readonly FasterList<ISubscription> _subscriptions = new();

        public bool IsInitialized { get; private set; }

        public Func<MessagePublisher> GetPublisher { get; set; }

        public Func<MessageSubscriber> GetSubscriber { get; set; }

        public Func<ArrayPool<UnityTask>> GetTaskArrayPool { get; set; }

        public MonoPageFlowContext Context => _context;

        public RectTransform RectTransform { get; private set; }

        public Canvas Canvas { get; private set; }

        public CanvasGroup CanvasGroup { get; private set; }

        public RectTransform PoolTransform { get; private set; }

        public static T Create<T>(string name, RectTransform parent, MonoPageFlowContext context)
            where T : MonoPageFlow
        {
            var root = new GameObject(
                  name
                , typeof(Canvas)
                , typeof(GraphicRaycaster)
                , typeof(CanvasGroup)
            );

            var rectTransform = root.GetOrAddComponent<RectTransform>();
            rectTransform.FillParent(parent);

            var flow = root.GetOrAddComponent<T>();
            flow.Initialize(parent, context);

            return flow;
        }

        public void Initialize(RectTransform parent = default, MonoPageFlowContext context = default)
        {
            RectTransform = this.GetOrAddComponent<RectTransform>();
            Canvas = this.GetOrAddComponent<Canvas>();
            CanvasGroup = this.GetOrAddComponent<CanvasGroup>();

            if (parent.IsValid())
                CreatePool(parent);
            else
                CreatePool(GetComponent<RectTransform>());

            context ??= _context;
            _context = context;

            context.Owner = this;

            if (context.IsInitialized == false)
            {
                MonoPageFlowSettings settings = null;

                if (context.useProjectSettings)
                {
                    settings = MonoPageFlowSettings.Instance;
                }

                var subscriber = GetSubscriber?.Invoke();
                var publisher = GetPublisher?.Invoke();
                var taskArrayPool = GetTaskArrayPool?.Invoke();

                context.Initialize(subscriber, publisher, settings, taskArrayPool);
            }

            var initContext = new InitializationContext(context.Subscriber, _subscriptions);
            SubscribeMessages(initContext);
            OnInitialize(initContext);

            IsInitialized = true;
        }

        protected virtual void OnInitialize(InitializationContext context) { }

        protected void Awake()
        {
            if (_context.autoInitializeOnAwake)
            {
                var rectTransform = GetComponent<RectTransform>();
                Initialize(rectTransform, _context);
            }
        }

        protected void OnDestroy()
        {
            _subscriptions.Unsubscribe();
            _pageIds.Clear();

            OnDispose();

            foreach (var pool in _pageIdToPool.Values)
            {
                pool.Dispose();
            }

            _pageIdToPool.Clear();
        }

        protected virtual void OnDispose() { }

        /// <summary>
        /// Preload an amount of view instances and keep them in the pool.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask PrepoolPageAsync(string pageAssetKey, int amount, CancellationToken token)
        {
            var pageKey = MakePageKey(pageAssetKey);
            return PrepoolPageAsync(pageKey, Mathf.Max(amount, 0), token);
        }

        /// <summary>
        /// Only keep a certain amount of pages in the pool, destroy the others.
        /// </summary>
        public void TrimPool(string pageAssetKey, int amountToKeep)
        {
            var pageKey = MakePageKey(pageAssetKey);
            TrimPool(pageKey, amountToKeep);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected PageKey MakePageKey([NotNull] string pageAssetKey)
        {
            ThrowIfNotInitialized(this);

            var id = Context.MakeStringId(pageAssetKey);
            return new(pageAssetKey, id);
        }

        /// <summary>
        /// Preload an amount of view instances and keep them in the pool.
        /// </summary>
        protected async UnityTask PrepoolPageAsync(PageKey pageKey, int amount, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (amount < 1)
            {
                WarningIfAmountLesserThanOne(this);
                return;
            }

            var pool = GetPoolFor(pageKey);

            if (pool.PoolingCount >= amount)
            {
                return;
            }

            if (pool.IsInitialized == false)
            {
                var sourceOpt = await Context.LoadAssetAsync(pageKey.Value, token);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (sourceOpt.TryValue(out var source) == false
                    || source.IsInvalid()
                )
                {
                    ErrorIfCannotLoadAsset(pageKey, this);
                    return;
                }

                pool.Initialize(source);
            }

            var differentAmount = amount - pool.PoolingCount;

            if (differentAmount < 1)
            {
                return;
            }

            pool.Prepool(differentAmount);
        }

        /// <summary>
        /// Only keep a certain amount of pages in the pool, destroy the others.
        /// </summary>
        protected void TrimPool(PageKey pageKey, int amountToKeep)
        {
            if (_pageIdToPool.TryGetValue(pageKey.Id, out var pool) == false)
            {
                return;
            }

            var amountToDestroy = pool.PoolingCount - Mathf.Clamp(amountToKeep, 0, pool.PoolingCount);

            if (amountToDestroy < 1)
            {
                return;
            }

            pool.Destroy(amountToDestroy);
        }

        protected async UnityTaskPageOpt RentPageAsync(PageKey pageKey, PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return default;
            }

            var pool = GetPoolFor(pageKey);

            if (pool.IsInitialized == false)
            {
                var sourceOpt = await Context.LoadAssetAsync(pageKey.Value, token);

                if (token.IsCancellationRequested)
                {
                    return default;
                }

                if (sourceOpt.TryValue(out var source) == false
                    || source.IsInvalid()
                )
                {
                    ErrorIfCannotLoadAsset(pageKey, this);
                    return default;
                }

                pool.Initialize(source);
            }

            var identifierOpt = pool.Rent(pageKey.Id, pageKey.Value, this.GetLogger(Context.logEnvironment));

            if (identifierOpt.TryValue(out var identifier) && identifier.Page is IPageCreateAsync create)
            {
                await create.OnCreateAsync(context, token);

                if (token.IsCancellationRequested)
                {
                    ReturnPageToPool(identifier, context);
                    return default;
                }
            }

            return identifierOpt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Option<MonoPageIdentifier> GetPageIdentifierAt(int index)
        {
            return (uint)index < (uint)_pageIds.Count
                ? _pageIds[index]
                : default(Option<MonoPageIdentifier>);
        }

        protected bool ReturnPageToPool(IMonoPage page, PageContext context)
        {
            if (page is not Component component)
            {
                ErrorIfPageIsNotComponent(page, this);
                return false;
            }

            if (component.TryGetComponent<MonoPageIdentifier>(out var identifier) == false)
            {
                ErrorIfPageMissingIdentifier(page, this);
                return false;
            }

            return ReturnPageToPool(identifier, context);
        }

        protected bool ReturnPageToPool(MonoPageIdentifier identifier, PageContext context)
        {
            if (identifier.IsInvalid() || identifier.Page is null)
            {
                return false;
            }

            if (identifier.Page is IPageTearDown teardown)
            {
                teardown.OnTearDown(context);
            }

            if (identifier.GameObjectId.IsValid == false || identifier.GameObject.IsInvalid())
            {
                ErrorIfPageIsDestroyedOrNotInited(identifier.Page, this);
                return false;
            }

            if (context.ReturnOperation == PageReturnOperation.Destroy)
            {
                Destroy(identifier.gameObject);
                return false;
            }

            if (_pageIdToPool.TryGetValue(identifier.KeyId, out var pool) == false)
            {
                ErrorIfCannotReturnToPool(identifier.AssetKey, this);
                return false;
            }

            pool.Return(identifier);
            return true;
        }

        private void CreatePool(RectTransform parent)
        {
            var poolGO = new GameObject(
                  $"[Pool] {name}"
                , typeof(Canvas)
                , typeof(CanvasGroup)
                , typeof(LayoutElement)
            ) {
                layer = 2, // Ignore Raycast
            };

            PoolTransform = poolGO.GetOrAddComponent<RectTransform>();
            PoolTransform.SetParent(parent, false);
            PoolTransform.FillParent(parent);

            var poolCanvasGroup = poolGO.GetComponent<CanvasGroup>();
            poolCanvasGroup.alpha = 0f;
            poolCanvasGroup.blocksRaycasts = false;
            poolCanvasGroup.interactable = false;

            var layoutElement = poolGO.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private MonoPagePool GetPoolFor(PageKey pageKey)
        {
            if (_pageIdToPool.TryGetValue(pageKey.Id, out var pool) == false)
            {
                _pageIdToPool[pageKey.Id] = pool = new MonoPagePool(PoolTransform, RectTransform, _pageIds);
            }

            return pool;
        }

        private void SubscribeMessages(InitializationContext context)
        {
            var subscriber = context.Subscriber.WithState(this);
            var subscriptions = context.Subscriptions;

            subscriber
                .Subscribe<PrepoolPageAsyncMessage>(static (state, msg, _, tkn) => state.HandleAsync(msg, tkn))
                .AddTo(subscriptions);

            subscriber
                .Subscribe<TrimPoolMessage>(static (state, msg) => state.Handle(msg))
                .AddTo(subscriptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnityTask HandleAsync(PrepoolPageAsyncMessage msg, CancellationToken token)
            => PrepoolPageAsync(msg.AssetKey, msg.Amount, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Handle(TrimPoolMessage msg)
            => TrimPool(msg.AssetKey, msg.AmountToKeep);

        [HideInCallstack]
        private static void ThrowIfNotInitialized(MonoPageFlow context)
        {
            if (context.IsInitialized == false)
            {
                throw new InvalidOperationException(
                    $"The page flow must be initialized via '{context.GetType()}.Initialize'. " +
                    $"Or the property 'Auto Initialize On Awake' must be checked on the Inspector window."
                );
            }
        }

        [HideInCallstack]
        private static void WarningIfAmountLesserThanOne(MonoPageFlow context)
        {
            context.GetLogger(context.Context.logEnvironment).LogWarning(
                $"The amount of preloaded instances should be greater than 0."
            );
        }

        [HideInCallstack]
        private static void ErrorIfCannotReturnToPool(string key, MonoPageFlow context)
        {
            context.GetLogger(context.Context.logEnvironment).LogError(
                $"Cannot return the page on the instance originated from '{key}' to pool. " +
                $"The pool may not be created properly."
            );
        }

        [HideInCallstack]
        private static void ErrorIfPageIsNotComponent(IMonoPage page, MonoPageFlow context)
        {
            context.GetLogger(context.Context.logEnvironment).LogError(
                $"The page '{page.GetType()}' is not derived from 'UnityEngine.Component'."
            );
        }

        [HideInCallstack]
        private static void ErrorIfPageMissingIdentifier(IMonoPage page, MonoPageFlow context)
        {
            context.GetLogger(context.Context.logEnvironment).LogError(
                $"Cannot found any {nameof(MonoPageIdentifier)} component on the page '{page.GetType()}'. " +
                $"The page might not be created correctly."
            );
        }

        [HideInCallstack]
        private static void ErrorIfPageIsDestroyedOrNotInited(IMonoPage page, MonoPageFlow context)
        {
            context.GetLogger(context.Context.logEnvironment).LogError(
                $"The page '{page.GetType()}' might have already been destroyed, " +
                $"or not properly initialized via pooling mechanism of MonoPageFlow."
            );
        }

        [HideInCallstack]
        private static void ErrorIfCannotLoadAsset(PageKey pageKey, MonoPageFlow context)
        {
            context.GetLogger(context.Context.logEnvironment).LogError(
                $"Cannot load asset by the key '{pageKey.Value}'"
            );
        }

        protected readonly record struct InitializationContext(
              MessageSubscriber.Subscriber<PageFlowScope> Subscriber
            , FasterList<ISubscription> Subscriptions
        );

        protected readonly record struct PageKey(string Value, StringId Id);
    }
}

#endif
