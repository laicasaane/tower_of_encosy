#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.Logging;

namespace EncosyTower.PubSub
{
    public readonly struct PublishingContext
    {
        public CallerInfo Caller { get; }

        public PublishingContext(in CallerInfo caller)
        {
            Caller = caller;
        }
    }
}

#endif
