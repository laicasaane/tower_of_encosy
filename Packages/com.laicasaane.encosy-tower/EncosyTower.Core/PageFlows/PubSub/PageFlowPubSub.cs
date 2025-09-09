#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Ids;
using EncosyTower.PubSub;
using EncosyTower.TypeWraps;

namespace EncosyTower.PageFlows
{
    [WrapRecord]
    public readonly partial record struct PageFlowScope(Id2 Value);

    public readonly record struct AttachPageMessage(IPageFlow Flow, IPage Page, CancellationToken Token) : IMessage;

    public readonly record struct DetachPageMessage(IPageFlow Flow, IPage Page, CancellationToken Token) : IMessage;

    public readonly record struct BeginTransitionMessage(IPage Previous, IPage Current, CancellationToken Token) : IMessage;

    public readonly record struct EndTransitionMessage(IPage Previous, IPage Current, CancellationToken Token) : IMessage;

}

#endif
