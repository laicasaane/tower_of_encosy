#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.PageFlows.MonoPages
{
    public abstract class MonoPageBase<TFlowScopes> : MonoPageBase
        , IPageNeedsFlowScopes<TFlowScopes>
        where TFlowScopes : struct, IPageFlowScopeCollection
    {
        private Option<TFlowScopes> _flowScopes;

        public Option<TFlowScopes> FlowScopes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flowScopes;
        }

        Option<TFlowScopes> IPageNeedsFlowScopes<TFlowScopes>.FlowScopes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _flowScopes = value;
        }
    }
}

#endif
