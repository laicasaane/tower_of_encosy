#if UNITASK || UNITY_6000_0_OR_NEWER

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV
#define __PUBSUB_INCLUDE_CALLER_INFO__
#endif
#endif

#if ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO
#define __PUBSUB_INCLUDE_CALLER_INFO__
#endif

using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskUntyped = Cysharp.Threading.Tasks.UniTask;
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTaskUntyped = UnityEngine.Awaitable;
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
    using UnityTask = UnityEngine.Awaitable;
#endif

    public sealed class PageFlow
    {
        private readonly ArrayPool<UnityTaskUntyped> _taskArrayPool;
        private readonly MessageSubscriber _subscriber;
        private readonly MessagePublisher _publisher;
        private readonly IPage _defaultPage;
        private readonly ILogger _logger;
        private readonly PageFlowScope _flowScope;
        private readonly Option<IPageFlowScopeCollectionApplier> _flowScopeCollectionApplier;
        private readonly bool _warnNoSubscriber;

        public PageFlow(
              [NotNull] ArrayPool<UnityTaskUntyped> taskArrayPool
            , MessageSubscriber subscriber
            , MessagePublisher publisher
            , PageFlowScope flowScope
            , Option<IPageFlowScopeCollectionApplier> flowScopeCollectionApplier
            , bool warnNoSubscriber
            , [NotNull] ILogger logger
        )
        {
            _taskArrayPool = taskArrayPool;
            _subscriber = subscriber;
            _publisher = publisher;
            _defaultPage = PageFlows.DefaultPage.Default;
            _logger = logger;
            _flowScope = flowScope;
            _flowScopeCollectionApplier = flowScopeCollectionApplier;
            _warnNoSubscriber = warnNoSubscriber;
        }

        public ArrayPool<UnityTaskUntyped> TaskArrayPool { get; }

        public ILogger Logger => _logger;

        public async UnityTaskBool AttachAsync(IPageFlow flow, IPage page, PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }

            InitializeIPageNeedsInterfaces(page, this);

            await _publisher.Scope(_flowScope).PublishAsync(
                  new AttachPageMessage(flow, page ?? _defaultPage, token)
                , GetPublishingContext()
                , token
            );

            if (token.IsCancellationRequested)
            {
                DeinitializeIPageNeedsInterfaces(page);
                return false;
            }

            if (page is IPageOnAttachToFlowAsync onAttach)
            {
                return await onAttach.OnAttachToFlowAsync(flow, context, token);
            }

            return true;
        }

        public async UnityTaskBool DetachAsync(IPageFlow flow, IPage page, PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }

            if (page is IPageOnDetachFromFlowAsync onDetach)
            {
                var detachResult = await onDetach.OnDetachFromFlowAsync(flow, context, token);

                if (detachResult == false)
                {
                    return false;
                }
            }

            await _publisher.Scope(_flowScope).PublishAsync(
                  new DetachPageMessage(flow, page ?? _defaultPage, token)
                , GetPublishingContext()
                , token
            );

            DeinitializeIPageNeedsInterfaces(page);

            return true;
        }

        public async UnityTaskBool PublishDetachAsync(IPageFlow flow, IPage page, CancellationToken token)
        {
            if (token.IsCancellationRequested == false)
            {
                await _publisher.Scope(_flowScope).PublishAsync(
                      new DetachPageMessage(flow, page ?? _defaultPage, token)
                    , GetPublishingContext()
                    , token
                );
            }
            else
            {
                return false;
            }

            return true;
        }

        public async UnityTaskBool TransitionAsync(
              PageTransition transition
            , IPage pageToHide
            , IPage pageToShow
            , PageContext context
            , CancellationToken token
        )
        {
            var taskArrayPool = _taskArrayPool;
            var parallelTasks = taskArrayPool.Rent(4);
            var sequentialTasks = taskArrayPool.Rent(2);
            var defaultPage = _defaultPage;
            var result = true;

            try
            {
                if (token.IsCancellationRequested == false)
                {
                    await _publisher.Scope(_flowScope).PublishAsync(
                          new BeginTransitionMessage(pageToHide ?? defaultPage, pageToShow ?? defaultPage, token)
                        , GetPublishingContext()
                        , token
                    );
                }
                else
                {
                    result = false;
                }

                context = ApplyPageOptionsToContext(transition, pageToHide, pageToShow, context);
                context = ProcessContext(pageToHide, pageToShow, context);

                IPageTransition pageToHideTransition = null;
                IPageTransition pageToShowTransition = null;

                if (pageToHide is IPageHasTransition hasHideTransition
                    && hasHideTransition.PageTransition is { } hideTransition
                )
                {
                    pageToHideTransition = hideTransition;
                }

                if (pageToShow is IPageHasTransition hasShowTransition
                    && hasShowTransition.PageTransition is { } showTransition
                )
                {
                    pageToShowTransition = showTransition;
                }

                if (token.IsCancellationRequested == false)
                {
                    await BeginTransitionAsync(
                          parallelTasks
                        , pageToHide
                        , pageToShow
                        , pageToHideTransition
                        , pageToShowTransition
                        , context
                        , token
                    );
                }
                else
                {
                    result = false;
                }

                if (token.IsCancellationRequested == false)
                {
                    if (context.SequentialTransition)
                    {
                        await SequentialTransitionAsync(
                              sequentialTasks
                            , pageToHide
                            , pageToShow
                            , pageToHideTransition
                            , pageToShowTransition
                            , context
                            , token
                        );
                    }
                    else
                    {
                        await ParallelTransitionAsync(
                              parallelTasks
                            , pageToHide
                            , pageToShow
                            , pageToHideTransition
                            , pageToShowTransition
                            , context
                            , token
                        );
                    }
                }
                else
                {
                    result = false;
                }

                if (token.IsCancellationRequested == false)
                {
                    (pageToShow as IPageOnAfterShow)?.OnAfterShow(context);

                    pageToShowTransition?.OnAfterTransition(
                          PageTransition.Show
                        , context.ShowOptions
                        , context.HideOptions
                    );

                    (pageToHide as IPageOnAfterHide)?.OnAfterHide(context);

                    pageToHideTransition?.OnAfterTransition(
                          PageTransition.Hide
                        , context.ShowOptions
                        , context.HideOptions
                    );

                    await _publisher.Scope(_flowScope).PublishAsync(
                          new EndTransitionMessage(pageToHide, pageToShow, token)
                        , GetPublishingContext()
                        , token
                    );
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            finally
            {
                taskArrayPool.Return(parallelTasks, true);
                taskArrayPool.Return(sequentialTasks, true);
            }

            return result;
        }

        private void InitializeIPageNeedsInterfaces(IPage page, PageFlow flow)
        {
            if (page is IPageNeedsFlowScope flowScope)
            {
                flowScope.FlowScope = _flowScope;
            }

            if (_flowScopeCollectionApplier.TryGetValue(out var applier))
            {
                applier.ApplyTo(page);
            }

            if (page is IPageNeedsMessageSubscriber subscriber)
            {
                subscriber.Subscriber = flow._subscriber;
            }

            if (page is IPageNeedsMessagePublisher publisher)
            {
                publisher.Publisher = flow._publisher;
            }
        }

        private static void DeinitializeIPageNeedsInterfaces(IPage page)
        {
            if (page is IPageNeedsFlowScope flowId)
            {
                flowId.FlowScope = default;
            }

            if (page is IPageNeedsMessageSubscriber subscriber)
            {
                subscriber.Subscriber = default;
            }

            if (page is IPageNeedsMessagePublisher publisher)
            {
                publisher.Publisher = default;
            }
        }

        private static PageContext ProcessContext(
              IPage pageToHide
            , IPage pageToShow
            , PageContext context
        )
        {
            context = context with {
                ShowOptions = TrySetZeroDuration(
                      context.ShowOptions
                    , PageTransitionOptions.OnlyFirstPageHasDuration
                    , pageToHide is not null || pageToShow is null
                )
            };

            context = context with {
                ShowOptions = TrySetZeroDuration(
                      context.ShowOptions
                    , PageTransitionOptions.OnlyLastPageHasDuration
                    , pageToHide is null || pageToShow is null
                )
            };

            context = context with {
                HideOptions = TrySetZeroDuration(
                      context.HideOptions
                    , PageTransitionOptions.OnlyFirstPageHasDuration
                    , pageToShow is not null || pageToHide is null
                )
            };

            context = context with {
                HideOptions = TrySetZeroDuration(
                      context.HideOptions
                    , PageTransitionOptions.OnlyLastPageHasDuration
                    , pageToShow is null || pageToHide is null
                )
            };

            return context;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static PageTransitionOptions TrySetZeroDuration(
                  PageTransitionOptions options
                , PageTransitionOptions checkValue
                , bool checkCondition
            )
            {
                if (options.Contains(checkValue) && checkCondition)
                {
                    options = options.Unset(checkValue);
                    options |= PageTransitionOptions.ZeroDuration;
                }

                return options;
            }
        }

        private static PageContext ApplyPageOptionsToContext(
              PageTransition transition
            , IPage pageToHide
            , IPage pageToShow
            , PageContext context
        )
        {
            PageOptions pageToHideOptions = (pageToHide as IPageHasOptions)?.PageOptions ?? default;
            PageOptions pageToShowOptions = (pageToShow as IPageHasOptions)?.PageOptions ?? default;

            var hideOptions = PageOptions.SelectHideOptions(transition, pageToHideOptions, pageToShowOptions)
                .GetTransitionOptions()
                .GetValueOrDefault(context.HideOptions);

            var showOptions = PageOptions.SelectShowOptions(transition, pageToHideOptions, pageToShowOptions)
                .GetTransitionOptions()
                .GetValueOrDefault(context.ShowOptions);

            return context with {
                ShowOptions = showOptions,
                HideOptions = hideOptions,
            };
        }

        private static async UnityTask BeginTransitionAsync(
              UnityTaskUntyped[] tasks
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext context
            , CancellationToken token
        )
        {
            PageTransitionOptions showOptions = context.ShowOptions;
            PageTransitionOptions hideOptions = context.HideOptions;

            tasks[0] = (pageToShow as IPageOnBeforeShowAsync)
                ?.OnBeforeShowAsync(context, token)
                ?? UnityTasks.GetCompleted();

            tasks[1] = pageToShowTransition
                ?.OnBeforeTransitionAsync(PageTransition.Show, showOptions, hideOptions, token)
                ?? UnityTasks.GetCompleted();

            tasks[2] = (pageToHide as IPageOnBeforeHideAsync)
                ?.OnBeforeHideAsync(context, token)
                ?? UnityTasks.GetCompleted();

            tasks[3] = pageToHideTransition
                ?.OnBeforeTransitionAsync(PageTransition.Hide, showOptions, hideOptions, token)
                ?? UnityTasks.GetCompleted();

            await UnityTasks.WhenAll(tasks);

            if (token.IsCancellationRequested)
            {
                return;
            }

            (pageToShow as IPageOnBeforeShow)?.OnBeforeShow(context);
            (pageToHide as IPageOnBeforeHide)?.OnBeforeHide(context);
        }

        private static async UnityTask ParallelTransitionAsync(
              UnityTaskUntyped[] tasks
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext context
            , CancellationToken token
        )
        {
            PageTransitionOptions showOptions = context.ShowOptions;
            PageTransitionOptions hideOptions = context.HideOptions;
            var withShow = showOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var withHide = hideOptions.Contains(PageTransitionOptions.NoTransition) == false;

            {
                tasks[0] = withShow && pageToShow is IPageOnShowAsync pageShow
                    ? pageShow.OnShowAsync(context, token)
                    : UnityTasks.GetCompleted();

                if (pageToShowTransition is not null)
                {
                    withShow |= pageToShowTransition.ForceRunShow;

                    tasks[1] = withShow
                        ? pageToShowTransition.OnShowAsync(showOptions, token)
                        : UnityTasks.GetCompleted();
                }
                else
                {
                    tasks[1] = UnityTasks.GetCompleted();
                }
            }

            {
                tasks[2] = withHide && pageToHide is IPageOnHideAsync pageHide
                    ? pageHide.OnHideAsync(context, token)
                    : UnityTasks.GetCompleted();

                if (pageToHideTransition is not null)
                {
                    withHide |= pageToHideTransition.ForceRunHide;

                    tasks[3] = withHide
                        ? pageToHideTransition.OnHideAsync(hideOptions, token)
                        : UnityTasks.GetCompleted();
                }
                else
                {
                    tasks[3] = UnityTasks.GetCompleted();
                }
            }

            if (withShow || withHide)
            {
                await UnityTasks.WhenAll(tasks);
            }
        }

        private static async UnityTask SequentialTransitionAsync(
              UnityTaskUntyped[] tasks
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext context
            , CancellationToken token
        )
        {
            PageTransitionOptions showOptions = context.ShowOptions;
            PageTransitionOptions hideOptions = context.HideOptions;
            var withShow = showOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var withHide = hideOptions.Contains(PageTransitionOptions.NoTransition) == false;

            {
                tasks[0] = withShow && pageToShow is IPageOnShowAsync pageShow
                    ? pageShow.OnShowAsync(context, token)
                    : UnityTasks.GetCompleted();

                if (pageToShowTransition is not null)
                {
                    withShow |= pageToShowTransition.ForceRunShow;

                    tasks[1] = withShow
                        ? pageToShowTransition.OnShowAsync(showOptions, token)
                        : UnityTasks.GetCompleted();
                }
                else
                {
                    tasks[1] = UnityTasks.GetCompleted();
                }

                await UnityTasks.WhenAll(tasks);
            }

            {
                tasks[0] = withHide && pageToHide is IPageOnHideAsync pageHide
                    ? pageHide.OnHideAsync(context, token)
                    : UnityTasks.GetCompleted();

                if (pageToHideTransition is not null)
                {
                    withHide |= pageToHideTransition.ForceRunHide;

                    tasks[1] = withHide
                        ? pageToHideTransition.OnHideAsync(hideOptions, token)
                        : UnityTasks.GetCompleted();
                }
                else
                {
                    tasks[1] = UnityTasks.GetCompleted();
                }

                await UnityTasks.WhenAll(tasks);
            }
        }

        private PublishingContext GetPublishingContext(
#if __PUBSUB_INCLUDE_CALLER_INFO__
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
#endif
        )
        {
            return PublishingContext.Default(
                  warnNoSubscriber: _warnNoSubscriber
                , logger: _logger
#if __PUBSUB_INCLUDE_CALLER_INFO__
                , callerInfo: new(lineNumber, memberName, filePath)
#endif
            );
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        public void LogWarningNoActivePage()
        {
            _logger.LogWarning("The flow has no active page.");
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        public void LogWarningNoPageToPop()
        {
            _logger.LogWarning("There is no page to pop.");
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        public void LogWarningNoPageToRemove()
        {
            _logger.LogWarning("There is no page to remove.");
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        public void LogWarningPageNotFound(IPage page)
        {
            _logger.LogWarning($"Cannot found this page '{page}' inside the flow.");
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        public void LogWarningIndexOutOfRange(int index)
        {
            _logger.LogWarning($"Index '{index}' is out of range.");
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        public void LogWarningCurrentPageIsIndex(int index)
        {
            _logger.LogWarning($"The page of index '{index}' is already showing.");
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        public void LogWarningCannotHideDefaultIndexPage(int index)
        {
            _logger.LogWarning($"Cannot hide the page whose index '{index}' is also the default index.");
        }
    }
}

#endif
