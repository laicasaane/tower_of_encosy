using Cysharp.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using UnityEngine;

namespace EncosyTower.Samples.MonoPages
{
    public class GamePageCodex : MonoBehaviour, IMonoPageCodexOnInitialize
    {
        private readonly PageFlowScopeCollectionApplier<GamePageFlowScopes> _flowScopesApplier = new();

        public IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier => _flowScopesApplier;

        public GamePageFlowScopes FlowScopes { get;  set; }

        private MonoPageCodex _codex;

        public UniTask OnInitializeAsync(MonoPageCodex codex)
        {
            _codex = codex;

            if (_flowScopesApplier.TryGetFlowScopeCollection(out var scopes))
            {
                ShowStartScreen(scopes);
            }

            return UniTask.CompletedTask;
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
