#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    public interface IPageFlow
    {
        bool IsInTransition { get; }
    }

    public interface IPageFlowScopeCollection
    {
    }

    public interface ISinglePageStack<TPage> : IPageFlow, IPageStackStrategy<TPage>
        where TPage : class, IPage
    {
        Option<TPage> CurrentPage { get; }
    }

    public interface IMultiPageStack<TPage> : IPageFlow, IPageStackStrategy<TPage>
        where TPage : class, IPage
    {
        Option<TPage> CurrentPage { get; }

        IReadOnlyCollection<TPage> Pages { get; }

        UnityTaskBool RemoveAllAsync(PageContext context, CancellationToken token);
    }

    public interface ISinglePageList<TPage> : IPageFlow, IPageListStrategy<TPage>
        where TPage : class, IPage
    {
        Option<TPage> CurrentPage { get; }

        IReadOnlyList<TPage> Pages { get; }

        UnityTaskBool HideAsync(PageContext context, CancellationToken token);
    }

    public interface IMultiPageList<TPage> : IPageFlow, IPageListStrategy<TPage>
        where TPage : class, IPage
    {
        IReadOnlyList<TPage> Pages { get; }

        UnityTaskBool HideAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTaskBool HideAsync(
#if UNITASK
              [NotNull] Func<CancellationToken, Cysharp.Threading.Tasks.UniTask<TPage>> factory
#else
              [NotNull] Func<CancellationToken, UnityEngine.Awaitable<TPage>> factory
#endif
            , PageContext context
            , CancellationToken token
        );

        UnityTaskBool HideAsync(int index, PageContext context, CancellationToken token);
    }
}

#endif
