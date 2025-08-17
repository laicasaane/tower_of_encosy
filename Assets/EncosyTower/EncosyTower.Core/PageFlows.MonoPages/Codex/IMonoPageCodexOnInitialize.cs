#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.PageFlows.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public interface IMonoPageCodexOnInitialize
    {
        IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier { get; }

        UnityTask OnInitializeAsync(MonoPageCodex codex);
    }
}

#endif
