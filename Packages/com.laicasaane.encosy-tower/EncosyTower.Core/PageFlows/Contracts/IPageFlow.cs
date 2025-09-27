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

    /// <summary>
    /// The type implements this interface must also define writable properties
    /// whose return type is <see cref="PageFlowScope"/>.
    /// </summary>
    /// <remarks>
    /// Because the system uses reflection mechanism to retrieve property information from this type,
    /// the type and all of its properties should be annotated with <c>[UnityEngine.Scripting.Preserve]</c>
    /// so their code will not be stripped away at build time.
    /// </remarks>
    /// <seealso cref="UnityEngine.Scripting.PreserveAttribute"/>
    /// <example>
    /// <code>
    /// [UnityEngine.Scripting.Preserve]
    /// public struct GameFlowScopes : IPageFlowScopeCollection
    /// {
    ///     [UnityEngine.Scripting.Preserve]
    ///     public PageFlowScope Screen { get; set; }
    ///
    ///     [UnityEngine.Scripting.Preserve]
    ///     public PageFlowScope Popup { get; set; }
    ///
    ///     [UnityEngine.Scripting.Preserve]
    ///     public PageFlowScope FreeTop { get; set; }
    /// }
    /// </code>
    /// </example>
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
