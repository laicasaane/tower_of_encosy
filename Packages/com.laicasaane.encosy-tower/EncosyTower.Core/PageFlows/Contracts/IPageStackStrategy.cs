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

    public interface IPageStackStrategy<TPage>
        where TPage : class, IPage
    {
        UnityTaskBool PopAsync(PageContext context, CancellationToken token);

        UnityTaskBool PushAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTaskBool PushAsync(
#if UNITASK
              [NotNull] Func<CancellationToken, Cysharp.Threading.Tasks.UniTask<TPage>> factory
#else
              [NotNull] Func<CancellationToken, UnityEngine.Awaitable<TPage>> factory
#endif
            , PageContext context
            , CancellationToken token
        );
    }
}

#endif
