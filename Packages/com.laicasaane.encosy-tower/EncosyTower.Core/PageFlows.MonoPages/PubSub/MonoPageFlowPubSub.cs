#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.PubSub;

namespace EncosyTower.PageFlows.MonoPages
{
    public readonly record struct PrepoolPageAsyncMessage(string AssetKey, int Amount) : IMessage;

    public readonly record struct TrimPoolMessage(string AssetKey, int AmountToKeep) : IMessage;

    public readonly record struct AddPageAsyncMessage(string AssetKey, PageContext Context) : IMessage;

    public readonly record struct ShowPageAsyncMessage(string AssetKey, PageContext Context) : IMessage;

    public readonly record struct ShowPageAtIndexAsyncMessage(int Index, PageContext Context) : IMessage;

    public readonly record struct HidePageAtIndexAsyncMessage(int Index, PageContext Context) : IMessage;

    public readonly record struct HideActivePageAsyncMessage(PageContext Context) : IMessage;
}

#endif
