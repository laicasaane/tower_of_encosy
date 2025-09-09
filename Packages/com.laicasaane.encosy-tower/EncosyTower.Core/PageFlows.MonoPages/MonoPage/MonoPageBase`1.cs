#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.PageFlows.MonoPages
{
    public abstract class MonoPageBase<TFlowScopeCollection> : MonoPageBase
        , IPageNeedsFlowScopeCollection<TFlowScopeCollection>
        where TFlowScopeCollection : struct, IPageFlowScopeCollection
    {
        private Option<TFlowScopeCollection> _flowScopeCollection;

        public Option<TFlowScopeCollection> FlowScopeCollection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flowScopeCollection;
        }

        Option<TFlowScopeCollection> IPageNeedsFlowScopeCollection<TFlowScopeCollection>.FlowScopeCollection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _flowScopeCollection = value;
        }
    }
}

#endif
