#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    public interface IPageFlowScopeCollectionApplier
    {
        Type FlowScopeCollectionType { get; }

        void SetFlowScopeCollection(object value);

        void ApplyTo(IPage page);
    }

    public sealed class PageFlowScopeCollectionApplier<TFlowScopes> : IPageFlowScopeCollectionApplier
        where TFlowScopes : struct, IPageFlowScopeCollection
    {
        private Option<TFlowScopes> _value;

        public bool TryGetFlowScopeCollection(out TFlowScopes result)
        {
            if (_value.TryGetValue(out result))
            {
                return true;
            }

            result = default;
            return false;
        }

        Type IPageFlowScopeCollectionApplier.FlowScopeCollectionType => typeof(TFlowScopes);

        void IPageFlowScopeCollectionApplier.ApplyTo(IPage page)
        {
            if (page is IPageNeedsFlowScopeCollection<TFlowScopes> collection)
            {
                collection.FlowScopeCollection = _value;
            }
        }

        void IPageFlowScopeCollectionApplier.SetFlowScopeCollection(object value)
        {
            if (value is TFlowScopes flowScopes)
            {
                _value = new(flowScopes);
            }
            else
            {
                _value = Option.None;
            }
        }
    }
}

#endif
