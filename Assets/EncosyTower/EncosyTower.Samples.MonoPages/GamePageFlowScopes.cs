using EncosyTower.PageFlows;

namespace EncosyTower.Samples.MonoPages
{
    public struct GamePageFlowScopes : IPageFlowScopeCollection
    {
        public PageFlowScope Screen { get; set; }

        public PageFlowScope Popup { get; set; }

        public PageFlowScope FreeTop { get; set; }
    }
}
