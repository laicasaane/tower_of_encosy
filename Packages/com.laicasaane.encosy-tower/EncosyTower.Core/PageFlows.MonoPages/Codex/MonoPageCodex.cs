#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    using UnityObject = UnityEngine.Object;

    public class MonoPageCodex : MonoBehaviour
    {
        [SerializeField] internal FlowDefinition[] _flows;
        [SerializeField] internal MonoPageFlowContext _flowContext = new();

        private readonly ArrayMap<string, PageFlowScope> _flowScopeMap = new();
        private readonly FasterList<MonoPageFlow> _monoPageFlows = new();
        private IPageFlowScopeCollectionApplier _flowScopeCollectionApplier;

        public MonoPageFlowContext FlowContext => _flowContext;

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

            var initializer = GetComponent<IMonoPageCodexOnInitialize>();

            if (initializer is not UnityObject initializerContext || initializerContext.IsInvalid())
            {
                ErrorIfCannotInitializeWithoutInitializer(this);
                return;
            }

            var flowScopeCollectionApplier = initializer.PageFlowScopeCollectionApplier;

            if (flowScopeCollectionApplier is null)
            {
                ErrorIfInitializerComponentReturnsNullApplier(this, initializerContext);
                return;
            }

            var flowScopeCollectionType = flowScopeCollectionApplier.FlowScopeCollectionType;

            if (flowScopeCollectionType == null)
            {
                ErrorIfApplierReturnsNullFlowScopeCollectionType(this, initializerContext);
                return;
            }

            if (flowScopeCollectionType.IsValueType == false
                || typeof(IPageFlowScopeCollection).IsAssignableFrom(flowScopeCollectionType) == false
            )
            {
                ErrorIfFlowScopeCollectionTypeIsInvalid(flowScopeCollectionType, this, initializerContext);
                return;
            }

            var properties = flowScopeCollectionType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(static x => x.CanWrite && x.PropertyType == typeof(PageFlowScope))
                .ToArray();

            if (properties.Length < 1)
            {
                ErrorIfFlowScopeCollectionTypeHasNoValidProperty(flowScopeCollectionType, this, initializerContext);
                return;
            }

            var definitions = _flows.AsSpan();

            if (ValidateFlowDefinitionList(flowScopeCollectionType, properties, definitions) == false)
            {
                return;
            }

            flowContext.FlowScopeCollectionApplier = new(flowScopeCollectionApplier);

            CreatePageFlows(flowContext, definitions);

            if (TryInitializeFlowScopeCollectionApplier(
                  flowScopeCollectionApplier
                , flowScopeCollectionType
                , properties
                , this
            ) == false)
            {
                return;
            }

            _flowScopeCollectionApplier = flowScopeCollectionApplier;
            initializer.OnInitializeAsync(this).Forget();
        }

        private bool ValidateFlowDefinitionList(
              Type type
            , ReadOnlySpan<PropertyInfo> properties
            , ReadOnlySpan<FlowDefinition> flows
        )
        {
            var flowIdentifierSet = new HashSet<string>(flows.Length);

            for (var i = 0; i < flows.Length; i++)
            {
                var definition = flows[i];
                var identifier = definition.identifier;

                if (identifier.IsNotEmpty())
                {
                    flowIdentifierSet.Add(identifier);
                    continue;
                }

                ErrorIfDefinitionIdentifierIsEmpty(i, this);
                return false;
            }

            var propertySet = new HashSet<string>(properties.Length);

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var identifier = property.Name;
                propertySet.Add(identifier);
            }

            if (propertySet.SetEquals(flowIdentifierSet) == false)
            {
                var missingIdentifiers = propertySet.Except(flowIdentifierSet);
                var missingIdentifiersString = string.Join(", ", missingIdentifiers);
                var missingProperties = flowIdentifierSet.Except(propertySet);
                var missingPropertiesString = string.Join(", ", missingProperties);
                ErrorIfFlowIdentifiersMismatch(type, missingIdentifiersString, missingPropertiesString, this);
                return false;
            }

            return true;
        }

        private void CreatePageFlows(
              MonoPageFlowContext originalContext
            , ReadOnlySpan<FlowDefinition> definitions
        )
        {
            var parent = GetComponent<RectTransform>();
            var length = definitions.Length;

            var flowScopeMap = _flowScopeMap;
            flowScopeMap.Clear();
            flowScopeMap.EnsureCapacity(length);

            var monoPageFlows = _monoPageFlows;
            monoPageFlows.Clear();
            monoPageFlows.IncreaseCapacityTo(length);

            var layer = gameObject.layer;

            for (var i = 0; i < length; i++)
            {
                var definition = definitions[i];
                var identifier = definition.identifier;
                var context = originalContext.CloneWithoutOwner();
                context.autoInitializeOnAwake = false;

                if (flowScopeMap.ContainsKey(identifier))
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

                if (flowScopeMap.TryAdd(identifier, scope) == false)
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
        }

        public bool TryGetFlowScope(string identifier, out PageFlowScope result)
            => _flowScopeMap.TryGetValue(identifier, out result);

        public Option<MonoPageFlow> GetFlowAt(int index)
        {
            return (uint)index < (uint)_monoPageFlows.Count
                ? _monoPageFlows[index]
                : Option.None;
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
            _flowScopeCollectionApplier = null;
            _flowScopeMap.Clear();
            _monoPageFlows.Clear();
        }

        private static bool TryInitializeFlowScopeCollectionApplier(
              IPageFlowScopeCollectionApplier flowScopeCollectionApplier
            , Type flowScopeCollectionType
            , ReadOnlySpan<PropertyInfo> properties
            , MonoPageCodex codex
        )
        {
            var map = codex._flowScopeMap;
            var flowScopeCollection = Activator.CreateInstance(flowScopeCollectionType);

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var identifier = property.Name;

                if (map.TryGetValue(identifier, out var flowScope) == false)
                {
                    ErrorIfCannotFindFlowScopeForProperty(flowScopeCollectionType, identifier, codex);
                    goto FAILED;
                }

                try
                {
                    property.SetValue(flowScopeCollection, flowScope);
                }
                catch (Exception ex)
                {
                    codex.GetLogger(codex._flowContext.logEnvironment).LogException(ex);
                    goto FAILED;
                }
            }

            flowScopeCollectionApplier.SetFlowScopeCollection(flowScopeCollection);
            return true;

        FAILED:
            return false;
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotInitializeWithoutInitializer(MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"Cannot initialize {nameof(MonoPageCodex)} without an initializer. " +
                $"Please add an {nameof(IMonoPageCodexOnInitialize)} component " +
                $"to the GameObject of this {nameof(MonoPageCodex)}."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfDefinitionIdentifierIsEmpty(int index, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"Cannot create a flow for definition at index {index} because its identifier is null or empty."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfUnexpectedErrorWhenCreate(int index, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"An unexpected error occured when create a flow for definition at index {index}."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfDuplicateIdentifier(int index, string identifier, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"The identifier '{identifier}' of definition at index {index} is duplicate thus will be ignored."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfUnexpectedErrorWhenRegister(int index, string identifier, MonoPageCodex context)
        {
            context.GetLogger(context._flowContext.logEnvironment).LogError(
                $"An unexpected error occured when register the flow '{identifier}' at index {index}."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfInitializerComponentReturnsNullApplier(MonoPageCodex codex, UnityObject context)
        {
            context.GetLogger(codex._flowContext.logEnvironment).LogError(
                $"The {nameof(IMonoPageCodexOnInitialize)} component " +
                $"returned a null {nameof(IPageFlowScopeCollectionApplier)}. " +
                $"Please ensure the interface {nameof(IMonoPageCodexOnInitialize)} is correctly implemented."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfApplierReturnsNullFlowScopeCollectionType(MonoPageCodex codex, UnityObject context)
        {
            context.GetLogger(codex._flowContext.logEnvironment).LogError(
                $"The {nameof(IPageFlowScopeCollectionApplier)} returned a null " +
                $"flow scope collection type. Please ensure the interface " +
                $"{nameof(IPageFlowScopeCollectionApplier)} is correctly implemented."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfFlowScopeCollectionTypeIsInvalid(Type type, MonoPageCodex codex, UnityObject context)
        {
            context.GetLogger(codex._flowContext.logEnvironment).LogError(
                $"The type '{type}' is an invalid Page Flow Scope Collection. " +
                $"It must be a struct and implement {nameof(IPageFlowScopeCollection)}."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfFlowScopeCollectionTypeHasNoValidProperty(Type type, MonoPageCodex codex, UnityObject context)
        {
            context.GetLogger(codex._flowContext.logEnvironment).LogError(
                $"The type '{type}' has no valid property to hold {nameof(PageFlowScope)} values. " +
                $"Please ensure the type implements {nameof(IPageFlowScopeCollection)} " +
                $"and has properties of type {nameof(PageFlowScope)}."
            );
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfFlowIdentifiersMismatch(
              Type type
            , string missingIdentifiers
            , string missingProperties
            , MonoPageCodex codex
        )
        {
            if (missingIdentifiers.IsNotEmpty())
            {
                codex.GetLogger(codex._flowContext.logEnvironment).LogError(
                    $"The flow definitions on this {nameof(MonoPageCodex)} " +
                    $"do not match the properties of type '{type}'. " +
                    $"The following identifiers are missing: {missingIdentifiers}. " +
                    $"Please specify the flow definitions according to the properties of type '{type}'."
                );
            }

            if (missingProperties.IsNotEmpty())
            {
                codex.GetLogger(codex._flowContext.logEnvironment).LogError(
                    $"The properties of type '{type}' do not match the identifiers " +
                    $"of the flow definitions on this {nameof(MonoPageCodex)}. " +
                    $"The following properties are missing: {missingProperties}. " +
                    $"Please ensure the properties of type '{type}' match the flow definitions."
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotFindFlowScopeForProperty(Type type, string identifier, MonoPageCodex codex)
        {
            codex.GetLogger(codex._flowContext.logEnvironment).LogError(
                $"Cannot find a flow scope value for the property '{identifier}' of the type '{type}'. " +
                $"Please add the flow definitions to this {nameof(MonoPageCodex)} " +
                $"matching the properties of type '{type}'."
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
    }
}

#endif
