#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Debugging;
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

    public readonly record struct PageFlow
    {
        public PageFlow(
              [NotNull] ArrayPool<UnityTaskUntyped> taskArrayPool
            , MessagePublisher.Publisher<PageFlowScope> publisher
            , [NotNull] ILogger logger
        )
        {
            Checks.IsTrue(publisher.IsValid, "Publisher must be created correctly.");

            TaskArrayPool = taskArrayPool;
            Publisher = publisher;
            Logger = logger;
        }

        public static IPage DefaultPage => PageFlows.DefaultPage.Default;

        public ArrayPool<UnityTaskUntyped> TaskArrayPool { get; private init; }

        public MessagePublisher.Publisher<PageFlowScope> Publisher { get; private init; }

        public ILogger Logger { get; private init; }

        [UnityEngine.HideInCallstack]
        public void LogWarningNoActivePage()
        {
            Logger.LogWarning("The flow has no active page.");
        }

        [UnityEngine.HideInCallstack]
        public void LogWarningNoPageToPop()
        {
            Logger.LogWarning("There is no page to pop.");
        }

        [UnityEngine.HideInCallstack]
        public void LogWarningNoPageToRemove()
        {
            Logger.LogWarning("There is no page to remove.");
        }

        [UnityEngine.HideInCallstack]
        public void LogWarningPageNotFound(IPage page)
        {
            Logger.LogWarning($"Cannot found this page '{page}' inside the flow.");
        }

        [UnityEngine.HideInCallstack]
        public void LogWarningIndexOutOfRange(int index)
        {
            Logger.LogWarning($"Index '{index}' is out of range.");
        }

        [UnityEngine.HideInCallstack]
        public void LogWarningCurrentPageIsIndex(int index)
        {
            Logger.LogWarning($"The page of index '{index}' is already showing.");
        }

        [UnityEngine.HideInCallstack]
        public void LogWarningCannotHideDefaultIndexPage(int index)
        {
            Logger.LogWarning($"Cannot hide the page whose index '{index}' is also the default index.");
        }

        public async UnityTaskBool AttachAsync(IPageFlow flow, IPage page, PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested == false)
            {
                await Publisher.PublishAsync(
                      new AttachPageMessage(flow, page ?? DefaultPage, token)
                    , PublishingContext.Get(true, Logger)
                    , token
                );
            }
            else
            {
                return false;
            }

            if (token.IsCancellationRequested == false)
            {
                if (page is IPageAttachToFlowAsync attach)
                {
                    await attach.OnAttachToFlowAsync(flow, context, token);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public async UnityTaskBool DetachAsync(IPageFlow flow, IPage page, PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested == false)
            {
                if (page is IPageDetachFromFlowAsync detach)
                {
                    await detach.OnDetachFromFlowAsync(flow, context, token);
                }
            }
            else
            {
                return false;
            }

            if (token.IsCancellationRequested == false)
            {
                await Publisher.PublishAsync(
                      new DetachPageMessage(flow, page ?? DefaultPage, token)
                    , PublishingContext.Get(true, Logger)
                    , token
                );
            }
            else
            {
                return false;
            }

            return true;
        }

        public async UnityTaskBool PublishDetachAsync(IPageFlow flow, IPage page, CancellationToken token)
        {
            if (token.IsCancellationRequested == false)
            {
                await Publisher.PublishAsync(
                      new DetachPageMessage(flow, page ?? DefaultPage, token)
                    , PublishingContext.Get(true, Logger)
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
              IPage pageToHide
            , IPage pageToShow
            , PageContext context
            , CancellationToken token
        )
        {
            var taskArrayPool = TaskArrayPool;
            var parallelTasks = taskArrayPool.Rent(4);
            var sequentialTasks = taskArrayPool.Rent(2);
            var defaultPage = DefaultPage;
            var result = true;

            context = context with {
                ShowOptions = TrySetZeroDuration(
                      context.ShowOptions
                    , PageTransitionOptions.OnlyFirstPageHasDuration
                    , (pageToHide is null && pageToShow is not null) == false
                )
            };

            context = context with {
                ShowOptions = TrySetZeroDuration(
                      context.ShowOptions
                    , PageTransitionOptions.OnlyLastPageHasDuration
                    , (pageToHide is not null && pageToShow is not null) == false
                )
            };

            context = context with {
                HideOptions = TrySetZeroDuration(
                      context.HideOptions
                    , PageTransitionOptions.OnlyFirstPageHasDuration
                    , (pageToShow is null && pageToHide is not null) == false
                )
            };

            context = context with {
                HideOptions = TrySetZeroDuration(
                      context.HideOptions
                    , PageTransitionOptions.OnlyLastPageHasDuration
                    , (pageToShow is not null && pageToHide is not null) == false
                )
            };

            try
            {
                if (token.IsCancellationRequested == false)
                {
                    await Publisher.PublishAsync(
                          new BeginTransitionMessage(pageToHide ?? defaultPage, pageToShow ?? defaultPage, token)
                        , PublishingContext.Get(true, Logger)
                        , token
                    );
                }
                else
                {
                    result = false;
                }

                IPageTransition pageToHideTransition = null;
                IPageTransition pageToShowTransition = null;

                if (pageToHide is ITryGet<IPageTransition> hideTransitionGetter)
                {
                    hideTransitionGetter.TryGet(out var transition);
                    pageToHideTransition = transition;
                }

                if (pageToShow is ITryGet<IPageTransition> showTransitionGetter)
                {
                    showTransitionGetter.TryGet(out var transition);
                    pageToShowTransition = transition;
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
                    {
                        var operation = PageTransitionOperation.Show;

                        (pageToShow as IPageAfterTransition)?.OnAfterTransition(operation, context);

                        pageToShowTransition?.OnAfterTransition(
                              operation
                            , context.ShowOptions
                            , context.HideOptions
                        );
                    }

                    {
                        var operation = PageTransitionOperation.Hide;

                        (pageToHide as IPageAfterTransition)?.OnAfterTransition(operation, context);

                        pageToHideTransition?.OnAfterTransition(
                              operation
                            , context.ShowOptions
                            , context.HideOptions
                        );
                    }

                    await Publisher.PublishAsync(
                          new EndTransitionMessage(pageToHide, pageToShow, token)
                        , PublishingContext.Get(true, Logger)
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
                Logger.LogException(ex);
            }
            finally
            {
                taskArrayPool.Return(parallelTasks, true);
                taskArrayPool.Return(sequentialTasks, true);
            }

            return result;
        }

        private static PageTransitionOptions TrySetZeroDuration(
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
            var showOptions = context.ShowOptions;
            var hideOptions = context.HideOptions;

            {
                var direction = PageTransitionOperation.Show;

                tasks[0] = (pageToShow as IPageBeforeTransitionAsync)
                    ?.OnBeforeTransitionAsync(direction, context, token)
                    ?? UnityTasks.GetCompleted();

                tasks[1] = pageToShowTransition
                    ?.OnBeforeTransitionAsync(direction, showOptions, hideOptions, token)
                    ?? UnityTasks.GetCompleted();
            }

            {
                var direction = PageTransitionOperation.Hide;

                tasks[2] = (pageToHide as IPageBeforeTransitionAsync)
                    ?.OnBeforeTransitionAsync(direction, context, token)
                    ?? UnityTasks.GetCompleted();

                tasks[3] = pageToHideTransition
                    ?.OnBeforeTransitionAsync(direction, showOptions, hideOptions, token)
                    ?? UnityTasks.GetCompleted();
            }

            await UnityTasks.WhenAll(tasks);
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
            var showOptions = context.ShowOptions;
            var hideOptions = context.HideOptions;
            var withShow = showOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var withHide = hideOptions.Contains(PageTransitionOptions.NoTransition) == false;

            {
                tasks[0] = withShow && pageToShow is IPageShowAsync pageShow
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
                tasks[2] = withHide && pageToHide is IPageHideAsync pageHide
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
            var showOptions = context.ShowOptions;
            var hideOptions = context.HideOptions;
            var withShow = showOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var withHide = hideOptions.Contains(PageTransitionOptions.NoTransition) == false;

            {
                tasks[0] = withShow && pageToShow is IPageShowAsync pageShow
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
                tasks[0] = withHide && pageToHide is IPageHideAsync pageHide
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
    }
}

#endif
