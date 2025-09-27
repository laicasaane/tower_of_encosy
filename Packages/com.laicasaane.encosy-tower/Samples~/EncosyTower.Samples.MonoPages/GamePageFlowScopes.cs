using EncosyTower.PageFlows;
using UnityEngine.Scripting;

namespace EncosyTower.Samples.MonoPages
{
    [Preserve]
    public struct GamePageFlowScopes : IPageFlowScopeCollection
    {
        [Preserve]
        public PageFlowScope Screen { get; set; }

        [Preserve]
        public PageFlowScope Popup { get; set; }

        [Preserve]
        public PageFlowScope FreeTop { get; set; }
    }
}
