#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.Samples.MonoPages
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public class GamePageCodex : MonoBehaviour, IMonoPageCodexOnInitialize
    {
        private readonly PageFlowScopeCollectionApplier<GamePageFlowScopes> _flowScopesApplier = new();

        public IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier => _flowScopesApplier;

        public GamePageFlowScopes FlowScopes { get; set; }

        private MonoPageCodex _codex;

        public UnityTask OnInitializeAsync(MonoPageCodex codex)
        {
            _codex = codex;

            if (_flowScopesApplier.TryGetFlowScopeCollection(out var scopes))
            {
                ShowStartScreen(scopes);
            }

            return UnityTasks.GetCompleted();
        }

        private void ShowStartScreen(GamePageFlowScopes scopes)
        {
            var publisher = _codex.FlowContext.Publisher.Scope(scopes.Screen);

            publisher.Publish(new ShowPageAsyncMessage("prefab-screen-red", new PageContext {
                ShowOptions = PageTransitionOptions.NoTransition,
            }));
        }
    }
}

#endif
