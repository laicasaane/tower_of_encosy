#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// Provides the necessary mechanism to create and set
    /// the value of a type that implements <see cref="IPageFlowScopeCollection"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="PageFlowScopeCollectionApplier{TFlowScopes}"/> should be used
    /// instead to minimize the user code.
    /// </remarks>
    public interface IPageFlowScopeCollectionApplier : ITrySet<IPageFlowScopeCollection>
    {
        /// <summary>
        /// The type of struct that implements <see cref="IPageFlowScopeCollection"/>.
        /// </summary>
        Type CollectionType { get; }

        /// <summary>
        /// Applies a value of <see cref="IPageFlowScopeCollection"/> to an <see cref="IPage"/>
        /// if that page also implements <see cref="IPageNeedsFlowScopeCollection{TCollection}"/>.
        /// </summary>
        /// <param name="page"></param>
        void ApplyTo(IPage page);
    }

    /// <summary>
    /// Provides a ready-to-use implementation of <see cref="IPageFlowScopeCollectionApplier"/>.
    /// </summary>
    /// <typeparam name="TCollection">
    /// The struct implements <see cref="IPageFlowScopeCollection"/>.
    /// </typeparam>
    /// <remarks>
    /// To simplify user code, this type should be used instead.
    /// </remarks>
    public sealed class PageFlowScopeCollectionApplier<TCollection>
        : IPageFlowScopeCollectionApplier
        , ITryGet<TCollection>
        where TCollection : struct, IPageFlowScopeCollection
    {
        private Option<TCollection> _value;

        /// <summary>
        /// Attempts to retrieve a value of <typeparamref name="TCollection"/>.
        /// </summary>
        /// <remarks>
        /// A codex system should be responsible to create this value and
        /// assign a <see cref="PageFlowScope"/> to each of its properties.
        /// </remarks>
        /// <param name="result">The value of <typeparamref name="TCollection"/> created by a codex system.</param>
        /// <returns>True if the value exists, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(out TCollection result)
            => _value.TryGetValue(out result);

        /// <inheritdoc/>
        Type IPageFlowScopeCollectionApplier.CollectionType => typeof(TCollection);

        /// <inheritdoc/>
        void IPageFlowScopeCollectionApplier.ApplyTo(IPage page)
        {
            if (page is IPageNeedsFlowScopeCollection<TCollection> collection)
            {
                collection.FlowScopeCollection = _value;
            }
        }

        bool ITrySet<IPageFlowScopeCollection>.TrySet(IPageFlowScopeCollection value)
        {
            if (value is TCollection flowScopes)
            {
                _value = new(flowScopes);
                return true;
            }
            else
            {
                _value = Option.None;
                return false;
            }
        }
    }
}

#endif
