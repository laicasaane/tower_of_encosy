#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.PageFlows
{
    internal sealed class DefaultPage : IPage
    {
        public static readonly IPage Default = new DefaultPage();
    }
}

#endif
