#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    public class MultiPageList<TPage> : IMultiPageList<TPage>, IDisposable
        where TPage : class, IPage
    {
        private readonly FasterList<TPage> _pages = new();
        private readonly ILogger _logger;
        private readonly PageFlow _flow;

        public MultiPageList([NotNull] IPageFlowContext context)
        {
            _logger = context.Logger ?? DevLogger.Default;
            _flow = new PageFlow(
                  context.TaskArrayPool
                , context.Subscriber
                , context.Publisher
                , context.FlowScope
                , context.FlowScopeCollectionApplier
                , context.WarnNoSubscriber
                , _logger
            );
        }

        public IReadOnlyList<TPage> Pages => _pages;

        public bool IsInTransition { get; private set; }

        public void Dispose()
        {
            _pages.Clear();
        }

        public int IndexOf([NotNull] TPage page)
            => _pages.IndexOf(page);

        public async UnityTaskBool AddAsync([NotNull] TPage page, PageContext context, CancellationToken token)
        {
            var result = await _flow.AttachAsync(this, page, context, token);

            if (token.IsCancellationRequested || result == false)
            {
                return false;
            }

            _pages.Add(page);
            return true;
        }

        public async UnityTaskBool AddAsync(
#if UNITASK
              [NotNull] Func<CancellationToken, Cysharp.Threading.Tasks.UniTask<TPage>> factory
#else
              [NotNull] Func<CancellationToken, UnityEngine.Awaitable<TPage>> factory
#endif
            , PageContext context
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }

            var page = await factory(token);
            return await AddAsync(page, context, token);
        }

        public async UnityTaskBool HideAsync([NotNull] TPage page, PageContext context, CancellationToken token)
        {
            var pages = _pages;
            var index = pages.IndexOf(page);

            if (index < 0)
            {
                _flow.LogWarningPageNotFound(page);
                return false;
            }

            return await HideAsync(index, context, token);
        }

        public async UnityTaskBool HideAsync(
#if UNITASK
              [NotNull] Func<CancellationToken, Cysharp.Threading.Tasks.UniTask<TPage>> factory
#else
              [NotNull] Func<CancellationToken, UnityEngine.Awaitable<TPage>> factory
#endif
            , PageContext context
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }

            var page = await factory(token);
            return await HideAsync(page, context, token);
        }

        public async UnityTaskBool HideAsync(int index, PageContext context, CancellationToken token)
        {
            var pages = _pages;
            var count = pages.Count;

            if ((uint)index >= (uint)count)
            {
                _flow.LogWarningIndexOutOfRange(index);
                return false;
            }

            if (token.IsCancellationRequested)
            {
                return false;
            }

            var validation = await Validator.ValidateTransitionAsync(this, _logger, context.AsyncOperation, token);

            if (validation == false)
            {
                return false;
            }

            if (token.IsCancellationRequested)
            {
                return false;
            }

            IsInTransition = true;

            var pageToHide = pages[index];
            var result = await _flow.TransitionAsync(PageTransition.Hide, pageToHide, default, context, token);

            IsInTransition = false;
            return result;
        }

        public async UnityTaskBool RemoveAllAsync(PageContext context, CancellationToken token)
        {
            var pages = _pages;
            var count = pages.Count;

            if (count < 1)
            {
                _flow.LogWarningNoPageToRemove();
                return false;
            }

            if (token.IsCancellationRequested)
            {
                return false;
            }

            var self = this;
            var flow = _flow;
            var taskArrayPool = flow.TaskArrayPool;
            var flowTasks = taskArrayPool.Rent(count);
            var pageTasks = taskArrayPool.Rent(count);
            var result = true;

            try
            {
                for (var index = 0; index < count; index++)
                {
                    var page = pages[index];
                    flowTasks[index] = flow.PublishDetachAsync(self, page, token).AsUnityTask();
                    pageTasks[index] = (page as IPageOnDetachFromFlowAsync)
                        ?.OnDetachFromFlowAsync(self, context, token).AsUnityTask()
                        ?? UnityTasks.GetCompleted();

                    index += 1;
                }

                if (token.IsCancellationRequested == false)
                {
                    await UnityTasks.WhenAll(flowTasks);
                }

                if (token.IsCancellationRequested == false)
                {
                    await UnityTasks.WhenAll(pageTasks);
                }
            }
            catch (Exception ex)
            {
                result = false;
                flow.Logger.LogException(ex);
            }
            finally
            {
                taskArrayPool.Return(flowTasks, true);
                taskArrayPool.Return(pageTasks, true);
            }

            if (token.IsCancellationRequested || result == false)
            {
                return false;
            }

            _pages.Clear();
            return true;
        }

        public async UnityTaskBool RemoveAsync([NotNull] TPage page, PageContext context, CancellationToken token)
        {
            var index = _pages.IndexOf(page);

            if (index < 0)
            {
                _flow.LogWarningPageNotFound(page);
                return false;
            }

            await _flow.DetachAsync(this, page, context, token);

            if (token.IsCancellationRequested)
            {
                return false;
            }

            _pages.RemoveAt(index);
            return true;
        }

        public async UnityTaskBool RemoveAsync(int index, PageContext context, CancellationToken token)
        {
            if ((uint)index >= (uint)_pages.Count)
            {
                _flow.LogWarningIndexOutOfRange(index);
                return false;
            }

            var page = _pages[index];

            await _flow.DetachAsync(this, page, context, token);

            if (token.IsCancellationRequested)
            {
                return false;
            }

            _pages.RemoveAt(index);
            return true;
        }

        public async UnityTaskBool ShowAsync([NotNull] TPage page, PageContext context, CancellationToken token)
        {
            var pages = _pages;
            var count = pages.Count;
            var index = pages.IndexOf(page);

            if (index < 0)
            {
                var result = await AddAsync(page, context, token);

                if (result == false)
                {
                    return false;
                }

                index = count - 1;
            }

            return await ShowAsync(index, context, token);
        }

        public async UnityTaskBool ShowAsync(
#if UNITASK
              [NotNull] Func<CancellationToken, Cysharp.Threading.Tasks.UniTask<TPage>> factory
#else
              [NotNull] Func<CancellationToken, UnityEngine.Awaitable<TPage>> factory
#endif
            , PageContext context
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }

            var page = await factory(token);
            return await ShowAsync(page, context, token);
        }

        public async UnityTaskBool ShowAsync(int index, PageContext context, CancellationToken token)
        {
            var pages = _pages;
            var count = pages.Count;

            if ((uint)index >= (uint)count)
            {
                _flow.LogWarningIndexOutOfRange(index);
                return false;
            }

            if (token.IsCancellationRequested)
            {
                return false;
            }

            var validation = await Validator.ValidateTransitionAsync(this, _logger, context.AsyncOperation, token);

            if (validation == false || token.IsCancellationRequested)
            {
                return false;
            }

            IsInTransition = true;

            var pageToShow = pages[index];
            var result = await _flow.TransitionAsync(PageTransition.Show, default, pageToShow, context, token);

            IsInTransition = false;
            return result;
        }
    }
}

#endif
