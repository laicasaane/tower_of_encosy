#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace EncosyTower.PageFlows
{
#if UNITASK
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    public interface IPageListStrategy<TPage>
        where TPage : class, IPage
    {
        int IndexOf(TPage page);

        UnityTaskBool AddAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTaskBool AddAsync(
#if UNITASK
              [NotNull] Func<CancellationToken, Cysharp.Threading.Tasks.UniTask<TPage>> factory
#else
              [NotNull] Func<CancellationToken, UnityEngine.Awaitable<TPage>> factory
#endif
            , PageContext context
            , CancellationToken token
        );

        UnityTaskBool RemoveAllAsync(PageContext context, CancellationToken token);

        UnityTaskBool RemoveAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTaskBool RemoveAsync(int index, PageContext context, CancellationToken token);

        UnityTaskBool ShowAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTaskBool ShowAsync(
#if UNITASK
              [NotNull] Func<CancellationToken, Cysharp.Threading.Tasks.UniTask<TPage>> factory
#else
              [NotNull] Func<CancellationToken, UnityEngine.Awaitable<TPage>> factory
#endif
            , PageContext context
            , CancellationToken token
        );

        UnityTaskBool ShowAsync(int index, PageContext context, CancellationToken token);
    }
}

#endif
