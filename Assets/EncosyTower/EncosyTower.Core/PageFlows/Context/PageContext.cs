#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.PageFlows
{
    public readonly record struct PageContext
    {
        public PageAsyncOperation AsyncOperation { get; init; }

        public PageReturnOperation ReturnOperation { get; init; }

        public PageTransitionOptions ShowOptions { get; init; }

        public PageTransitionOptions HideOptions { get; init; }

        public bool SequentialTransition { get; init; }

        public int DefaultIndex { get; init; }

        public object UserData { get; init; }
    }
}

#endif
