#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Debugging;
using EncosyTower.Logging;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    public class SinglePageStack<TPage> : ISinglePageStack<TPage>, IDisposable
        where TPage : class, IPage
    {
        private readonly ConcurrentStack<TPage> _pages = new();
        private readonly ILogger _logger;
        private readonly PageFlow _flow;

        public SinglePageStack([NotNull] IPageFlowContext context)
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

        public Option<TPage> CurrentPage => _pages.TryPeek(out var page) ? page : default(Option<TPage>);

        public bool IsInTransition { get; private set; }

        public void Dispose()
        {
            _pages.Clear();
        }

        public async UnityTaskBool PopAsync(PageContext context, CancellationToken token)
        {
            if (_pages.Count < 1)
            {
                _flow.LogWarningNoPageToPop();
                return default;
            }

            if (token.IsCancellationRequested)
            {
                return default;
            }

            var validation = await Validator.ValidateTransitionAsync(this, _logger, context.AsyncOperation, token);

            if (validation == false || token.IsCancellationRequested)
            {
                return default;
            }

            IsInTransition = true;

            _pages.TryPop(out var pageToHide);
            _pages.TryPeek(out var pageToShow);

            var result = await _flow.TransitionAsync(PageTransition.Hide, pageToHide, pageToShow, context, token);

            if (result)
            {
                result = await _flow.DetachAsync(this, pageToHide, context, token);
            }

            if (result == false && pageToHide is not null)
            {
                _pages.Push(pageToHide);
            }

            IsInTransition = false;
            return result;
        }

        public async UnityTaskBool PushAsync([NotNull] TPage page, PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return default;
            }

            var validation = await Validator.ValidateTransitionAsync(this, _logger, context.AsyncOperation, token);

            if (validation == false || token.IsCancellationRequested)
            {
                return default;
            }

            IsInTransition = true;

            _pages.TryPop(out var pageToHide);
            _pages.Push(page);

            var result = await _flow.AttachAsync(this, page, context, token);

            if (result)
            {
                result = await _flow.TransitionAsync(PageTransition.Show, pageToHide, page, context, token);
            }

            if (result && pageToHide is not null)
            {
                result = await _flow.DetachAsync(this, pageToHide, context, token);
            }

            if (result == false)
            {
                _pages.TryPop(out _);

                if (pageToHide is not null)
                {
                    _pages.Push(pageToHide);
                }
            }

            IsInTransition = false;
            return result;
        }

        public async UnityTaskBool PushAsync(
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
                return default;
            }

            var page = await factory(token);
            return await PushAsync(page, context, token);
        }
    }
}

#endif
