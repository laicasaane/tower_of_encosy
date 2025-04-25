#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using System.Threading;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.ResourceKeys;
using EncosyTower.StringIds;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITY_ADDRESSABLES
    using EncosyTower.AddressableKeys;
    using EncosyTower.Processing;
    using EncosyTower.UnityExtensions;
#endif

#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
    using UnityTaskObject = Cysharp.Threading.Tasks.UniTask<Option<GameObject>>;
#else
    using UnityTask = UnityEngine.Awaitable;
    using UnityTaskObject = UnityEngine.Awaitable<Option<GameObject>>;
#endif

    [Serializable]
    public sealed class MonoPageFlowContext : IPageFlowContext
    {
        public bool autoInitializeOnAwake;
        public bool useProjectSettings = true;
        public bool slimPublishingContext = true;
        public bool ignoreEmptySubscriber = true;
        public MonoPageLoaderStrategy loadStrategy;
        public MonoMessageScope messageScope;
        public LogEnvironment logEnvironment;

        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private Processor _processor;
        private ArrayPool<UnityTask> _taskArrayPool;

        public Component Owner { get; set; }

        public bool IsInitialized { get; private set; }

        public ArrayPool<UnityTask> TaskArrayPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _taskArrayPool;
        }

        public PageFlowScope FlowScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => messageScope == MonoMessageScope.Component
                ? new(new((Id<MonoPageFlow>)Type<MonoPageFlow>.Id, Owner.GetInstanceID()))
                : new(new((Id<GameObject>)Type<GameObject>.Id, Owner.gameObject.GetInstanceID()));
        }

        public MessageSubscriber.Subscriber<PageFlowScope> Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subscriber.Scope(FlowScope);
        }

        public MessagePublisher.Publisher<PageFlowScope> Publisher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _publisher.Scope(FlowScope);
        }

        public ProcessHub<PageFlowScope> ProcessHub
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _processor.Scope(FlowScope);
        }

        public bool SlimPublishingContext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => slimPublishingContext;
        }

        public bool IgnoreEmptySubscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ignoreEmptySubscriber;
        }

        public Logging.ILogger Logger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => logEnvironment == LogEnvironment.Runtime ? RuntimeLogger.Default : DevLogger.Default;
        }

        public void Initialize(
              MessageSubscriber subscriber = null
            , MessagePublisher publisher = null
            , Processor processor = null
            , MonoPageFlowSettings settings = null
            , ArrayPool<UnityTask> taskArrayPool = null
        )
        {
            IsInitialized = true;

            if (settings.IsValid() && useProjectSettings)
            {
                slimPublishingContext = settings.slimPublishingContext;
                ignoreEmptySubscriber = settings.ignoreEmptySubscriber;
                loadStrategy = settings.loaderStrategy;
                messageScope = settings.messageScope;
                logEnvironment = settings.logEnvironment;
                Logger.LogInfo($"Initialize {nameof(MonoPageFlowContext)} using Project Settings");
            }

            _subscriber = subscriber ?? GlobalMessenger.Subscriber;
            _publisher = publisher ?? GlobalMessenger.Publisher;
            _processor = processor ?? GlobalProcessor.Instance;
            _taskArrayPool = taskArrayPool ?? ArrayPool<UnityTask>.Shared;
        }

        public MonoPageFlowContext CloneWithoutOwner()
            => new() {
                autoInitializeOnAwake = autoInitializeOnAwake,
                useProjectSettings = useProjectSettings,
                slimPublishingContext = slimPublishingContext,
                ignoreEmptySubscriber = ignoreEmptySubscriber,
                loadStrategy = loadStrategy,
                messageScope = messageScope,
                logEnvironment = logEnvironment,
                _subscriber = _subscriber,
                _publisher = _publisher,
                _processor = _processor,
                _taskArrayPool = _taskArrayPool,
                IsInitialized = IsInitialized,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringId MakeStringId(string str)
            => StringToId.MakeFromManaged(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(StringId id)
            => IdToString.GetManaged(id);

        public async UnityTaskObject LoadAssetAsync(string assetKey, CancellationToken token)
        {
#if UNITY_ADDRESSABLES
            if (loadStrategy == MonoPageLoaderStrategy.Addressables)
            {
                return await new AddressableKey(assetKey).TryLoadAsync<GameObject>(token);
            }
#endif

            return await new ResourceKey(assetKey).TryLoadAsync<GameObject>(token);
        }
    }
}

#endif
