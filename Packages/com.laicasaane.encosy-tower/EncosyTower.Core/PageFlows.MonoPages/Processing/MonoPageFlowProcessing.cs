#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Processing;

namespace EncosyTower.PageFlows.MonoPages
{
    public readonly record struct IsInTransitionRequest() : IRequest<bool>;

    public readonly record struct GetPageIndexRequest(IMonoPage Page) : IRequest<int>;

    public readonly record struct GetCurrentPageRequest() : IRequest<Option<IMonoPage>>;

    public readonly record struct GetPageListRequest() : IRequest<IReadOnlyList<IMonoPage>>;

    public readonly record struct GetPageCollectionRequest() : IRequest<IReadOnlyCollection<IMonoPage>>;
}

#endif
