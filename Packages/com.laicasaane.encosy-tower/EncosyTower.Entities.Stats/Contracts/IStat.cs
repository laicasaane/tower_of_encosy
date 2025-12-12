using Unity.Entities;

namespace EncosyTower.Entities.Stats
{
    public interface IStat<TValuePair> : IBufferElementData
        where TValuePair : unmanaged, IStatValuePair
    {
        ModifierRange ModifierRange { get; set; }

        ObserverRange ObserverRange { get; set; }

        bool ProduceChangeEvents { get; set; }

        TValuePair ValuePair { get; set; }

        /// <summary>
        /// To store user-defined values.
        /// </summary>
        /// <remarks>
        /// Must be either <see cref="byte"/>, <see cref="ushort"/>, or <see cref="uint"/>.
        /// </remarks>
        uint UserData { get; set; }

        /// <summary>
        /// Size of the underlying <see cref="UserData"/> in bytes.
        /// </summary>
        /// <remarks>
        /// Must equal to the size of either <see cref="byte"/>, <see cref="ushort"/>, or <see cref="uint"/>.
        /// </remarks>
        int UserDataSize { get; }

        StatVariant GetBaseValueOrDefault(in StatVariant defaultValue = default);

        StatVariant GetCurrentValueOrDefault(in StatVariant defaultValue = default);

        bool TrySetBaseValue(in StatVariant value);

        bool TrySetCurrentValue(in StatVariant value);

        bool TrySetValues(in StatVariant baseValue, in StatVariant currentValue);
    }
}
