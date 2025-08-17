#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public class MonoPageCodex : MonoBehaviour
    {
        [SerializeField] internal FlowDefinition[] _flows;
        [SerializeField] internal MonoPageFlowContext _flowContext = new();

        private readonly ArrayMap<string, FlowScopeRecord> _recordMap = new();
        private readonly FasterList<MonoPageFlow> _monoPageFlows = new();

        public MonoPageFlowContext FlowContext => _flowContext;

        public bool IsInitialized { get; private set; }

        public void Initialize(
              MonoPageFlowContext flowContext = null
            , Func<MessagePublisher> getPublisherFunc = null
            , Func<MessageSubscriber> getSubscriberFunc = null
            , Func<Processor> getProcessorFunc = null
            , Func<ArrayPool<UnityTask>> getTaskArrayPoolFunc = null
        )
        {
            flowContext ??= _flowContext;
            _flowContext = flowContext;

            flowContext.Owner = this;

            if (flowContext.IsInitialized == false)
            {
                MonoPageFlowSettings settings = null;

                if (flowContext.useProjectSettings)
                {
                    settings = MonoPageFlowSettings.Instance;
                }

                var subscriber = getSubscriberFunc?.Invoke();
                var publisher = getPublisherFunc?.Invoke();
                var processor = getProcessorFunc?.Invoke();
                var taskArrayPool = getTaskArrayPoolFunc?.Invoke();

                flowContext.Initialize(subscriber, publisher, processor, settings, taskArrayPool);
            }

            var definitions = _flows.AsSpan();
            var length = definitions.Length;

            if (length < 1)
            {
                return;
            }

            var parent = GetComponent<RectTransform>();
            var recordMap = _recordMap;
            recordMap.EnsureCapacity(length);

            var monoPageFlows = _monoPageFlows;
            monoPageFlows.IncreaseCapacityTo(length);

            var layer = gameObject.layer;

            for (var i = 0; i < length; i++)
            {
                var definition = definitions[i];
                var identifier = definition.identifier;

                var context = flowContext.CloneWithoutOwner();
                context.autoInitializeOnAwake = false;

                if (string.IsNullOrEmpty(identifier))
                {
                    ErrorIfDefinitionIdentifierIsEmpty(i, this);
                    continue;
                }

                if (recordMap.ContainsKey(identifier))
                {
                    ErrorIfDuplicateIdentifier(i, identifier, this);
                    continue;
                }

                Option<MonoPageFlow> flowOpt = definition.kind switch {
                    MonoPageFlowKind.SinglePageStack => MonoPageFlow.Create<MonoSinglePageStack>(identifier, parent, context),
                    MonoPageFlowKind.MultiPageStack => MonoPageFlow.Create<MonoMultiPageStack>(identifier, parent, context),
                    MonoPageFlowKind.SinglePageList => MonoPageFlow.Create<MonoSinglePageList>(identifier, parent, context),
                    MonoPageFlowKind.MultiPageList => MonoPageFlow.Create<MonoMultiPageList>(identifier, parent, context),
                    _ => Option.None,
                };

                if (flowOpt.TryGetValue(out var flow) == false)
                {
                    ErrorIfUnexpectedErrorWhenCreate(i, this);
                    continue;
                }

                var scope = flow.Context.FlowScope;
                var index = monoPageFlows.Count;

                if (recordMap.TryAdd(identifier, new(scope, index)) == false)
                {
                    ErrorIfUnexpectedErrorWhenRegister(i, identifier, this);
                    Destroy(flow.gameObject);
                    continue;
                }

                monoPageFlows.Add(flow);

                if (definition.overrideSortingLayer == false)
                {
                    continue;
                }

                var canvas = flow.Canvas;
                canvas.gameObject.layer = layer;
                canvas.overrideSorting = true;
                canvas.sortingLayerID = definition.sortingLayer;
                canvas.sortingOrder = definition.sortingOrderInLayer;
            }

            IsInitialized = true;
        }

        public bool TryGetFlowScopeRecord(string identifier, out FlowScopeRecord result)
            => _recordMap.TryGetValue(identifier, out result);

        public Option<MonoPageFlow> GetFlowAt(int index)
        {
            return (uint)index < (uint)_monoPageFlows.Count
                ? _monoPageFlows[index]
                : default(Option<MonoPageFlow>);
        }

        private void Awake()
        {
            if (_flowContext.autoInitializeOnAwake)
            {
                Initialize(_flowContext);
            }
        }

        private void OnDestroy()
        {
            _monoPageFlows.Clear();
        }

        [HideInCallstack]
        private static void ErrorIfDefinitionIdentifierIsEmpty(int index, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"Cannot create a flow for definition at index {index} because its identifier is null or empty."
            );
        }

        [HideInCallstack]
        private static void ErrorIfUnexpectedErrorWhenCreate(int index, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"An unexpected error occured when create a flow for definition at index {index}."
            );
        }

        [HideInCallstack]
        private static void ErrorIfDuplicateIdentifier(int index, string identifier, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"The identifier '{identifier}' of definition at index {index} is duplicate thus will be ignored."
            );
        }

        [HideInCallstack]
        private static void ErrorIfUnexpectedErrorWhenRegister(int index, string identifier, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"An unexpected error occured when register the flow '{identifier}' at index {index}."
            );
        }

        [Serializable]
        internal struct FlowDefinition
        {
            public string identifier;
            public MonoPageFlowKind kind;
            public bool overrideSortingLayer;
            public SortingLayerId sortingLayer;
            public int sortingOrderInLayer;
        }

        public readonly record struct FlowScopeRecord(PageFlowScope Scope, int Index);
    }
}

#endif
