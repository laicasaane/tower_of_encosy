#if UNITASK || UNITY_6000_0_OR_NEWER

using Module.Core.Logging;

namespace Module.Core.PubSub
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
