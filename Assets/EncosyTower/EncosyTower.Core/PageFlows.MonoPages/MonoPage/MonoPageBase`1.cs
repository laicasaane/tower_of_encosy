#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.Common;

namespace EncosyTower.PageFlows.MonoPages
{
    public abstract class MonoPageBase<TFlowScopes> : MonoPageBase
        , IPageNeedsFlowScopes<TFlowScopes>
        where TFlowScopes : struct, IPageFlowScopeCollection
    {
        public Option<TFlowScopes> FlowScopes { get; set; }
    }
}

#endif
