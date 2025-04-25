#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Debugging;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    public class SinglePageList<TPage> : ISinglePageList<TPage>, IDisposable
        where TPage : class, IPage
    {
        private readonly FasterList<TPage> _pages = new();
        private readonly ILogger _logger;
        private readonly PageFlow _flow;

        public SinglePageList([NotNull] IPageFlowContext context)
        {
            var publisher = context.Publisher;

            Checks.IsTrue(publisher.IsValid, "Publisher must be created correctly.");

            _logger = context.Logger ?? DevLogger.Default;
            _flow = new PageFlow(
                  context.TaskArrayPool
                , publisher
                , context.SlimPublishingContext
                , context.IgnoreEmptySubscriber
                , _logger
            );
        }

        public Option<TPage> CurrentPage { get; private set; }

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

        public async UnityTaskBool HideAsync(PageContext context, CancellationToken token)
        {
            if (CurrentPage.TryValue(out var pageToHide) == false)
            {
                _flow.LogWarningNoActivePage();
                return false;
            }

            var pages = _pages;
            var index = pages.IndexOf(pageToHide);

            if (index < 0)
            {
                _flow.LogWarningPageNotFound(pageToHide);
                return false;
            }

            if (index == context.DefaultIndex)
            {
                _flow.LogWarningCannotHideDefaultIndexPage(index);
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
            CurrentPage = default;

            var pageToShow = (uint)context.DefaultIndex < (uint)pages.Count
                ? pages[context.DefaultIndex]
                : default;

            var result = await _flow.TransitionAsync(PageTransition.Hide, pageToHide, pageToShow, context, token);

            CurrentPage = result ? pageToShow : pageToHide;
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
                    flowTasks[index] = flow.PublishDetachAsync(self, page, token);
                    pageTasks[index] = (page as IPageDetachFromFlowAsync)
                        ?.OnDetachFromFlowAsync(self, context, token)
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

            var result = await _flow.DetachAsync(this, page, context, token);

            if (token.IsCancellationRequested || result == false)
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
            var result = await _flow.DetachAsync(this, page, context, token);

            if (token.IsCancellationRequested || result == false)
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

            var pageToShow = pages[index];

            if (ReferenceEquals(pageToShow, CurrentPage))
            {
                _flow.LogWarningCurrentPageIsIndex(index);
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

            var pageToHide = CurrentPage.ValueOrDefault();
            var result = await _flow.TransitionAsync(PageTransition.Show, pageToHide, pageToShow, context, token);

            CurrentPage = result ? pageToShow : pageToHide;
            IsInTransition = false;
            return result;
        }
    }
}

#endif
