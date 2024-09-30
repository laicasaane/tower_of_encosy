#if UNITASK || UNITY_6000_0_OR_NEWER

namespace Module.Core.PubSub
{
    /// <summary>
    /// <para>In strict (default) mode, messages must implement <see cref="IMessage"/> interface.</para>
    /// <para>To disable this mode, add <c>MODULE_CORE_PUBSUB_RELAX_MODE</c> to</para>
    /// <para>Project Settings > Player > Scripting Define Symbols</para>
    /// </summary>
    public interface IMessage { }
}

#endif
