#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.Modules.PubSub
{
    /// <summary>
    /// Anonymous message type to support <see cref="AnonSubscriber"/> and <see cref="AnonPublisher"/>
    /// </summary>
    public readonly struct AnonMessage : IMessage { }
}

#endif
