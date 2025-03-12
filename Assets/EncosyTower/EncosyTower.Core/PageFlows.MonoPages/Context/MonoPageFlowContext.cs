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
        [SerializeField] private bool _autoInitializeOnAwake;
        [SerializeField] private bool _useProjectSettings = true;
        [SerializeField] private MonoPageLoaderStrategy _loadStrategy;
        [SerializeField] private MonoMessageScope _messageScope;
        [SerializeField] private LogEnvironment _logEnvironment;

        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private ArrayPool<UnityTask> _taskArrayPool;

        public Component Owner { get; set; }

        public bool IsInitialized { get; private set; }

        public bool UseProjectSettings
        {
            get => _useProjectSettings;
            set => _useProjectSettings = value;
        }

        public bool AutoInitializeOnAwake
        {
            get => _autoInitializeOnAwake;
            set => _autoInitializeOnAwake = value;
        }

        public MonoPageLoaderStrategy LoadStrategy
        {
            get => _loadStrategy;
            set => _loadStrategy = value;
        }

        public MonoMessageScope MessageScope
        {
            get => _messageScope;
            set => _messageScope = value;
        }

        public LogEnvironment LogEnvironment
        {
            get => _logEnvironment;
            set => _logEnvironment = value;
        }

        public ArrayPool<UnityTask> TaskArrayPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _taskArrayPool;
        }

        public PageFlowScope FlowScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _messageScope == MonoMessageScope.Component
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

        public Logging.ILogger Logger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _logEnvironment == LogEnvironment.Runtime ? RuntimeLogger.Default : DevLogger.Default;
        }

        public void Initialize(
              MessageSubscriber subscriber = null
            , MessagePublisher publisher = null
            , MonoPageFlowSettings settings = null
            , ArrayPool<UnityTask> taskArrayPool = null
        )
        {
            IsInitialized = true;

            if (settings.IsValid() && _useProjectSettings)
            {
                _loadStrategy = settings.LoaderStrategy;
                _messageScope = settings.MessageScope;
                _logEnvironment = settings.LogEnvironment;
                Logger.LogInfo($"Initialize {nameof(MonoPageFlowContext)} using Project Settings");
            }

            _subscriber = subscriber ?? GlobalMessenger.Subscriber;
            _publisher = publisher ?? GlobalMessenger.Publisher;
            _taskArrayPool = taskArrayPool ?? ArrayPool<UnityTask>.Shared;
        }

        public MonoPageFlowContext CloneWithoutOwner()
            => new() {
                _autoInitializeOnAwake = _autoInitializeOnAwake,
                _useProjectSettings = _useProjectSettings,
                _loadStrategy = _loadStrategy,
                _messageScope = _messageScope,
                _logEnvironment = _logEnvironment,
                _subscriber = _subscriber,
                _publisher = _publisher,
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
            if (_loadStrategy == MonoPageLoaderStrategy.Addressables)
            {
                return await new AddressableKey(assetKey).TryLoadAsync<GameObject>(token);
            }
#endif

            return await new ResourceKey(assetKey).TryLoadAsync<GameObject>(token);
        }
    }
}

#endif
