#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.PubSub
{
    public enum PublishingStrategy : byte
    {
        DropIfNoSubscriber = 0,
        WaitForSubscriber,
    }
}

#endif
