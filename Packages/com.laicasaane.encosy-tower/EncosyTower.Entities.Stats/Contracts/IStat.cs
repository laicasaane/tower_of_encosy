using Unity.Entities;

namespace EncosyTower.Entities.Stats
{
    public interface IStat<TValuePair> : IBufferElementData
        where TValuePair : unmanaged, IStatValuePair
    {
        StatHandle Handle { get; set; }

        ModifierRange ModifierRange { get; set; }

        ObserverRange ObserverRange { get; set; }

        bool ProduceChangeEvents { get; set; }

        TValuePair ValuePair { get; set; }

        StatVariant GetBaseValueOrDefault(in StatVariant defaultValue = default);

        StatVariant GetCurrentValueOrDefault(in StatVariant defaultValue = default);

        bool TrySetBaseValue(in StatVariant value);

        bool TrySetCurrentValue(in StatVariant value);

        bool TrySetValues(in StatVariant baseValue, in StatVariant currentValue);
    }
}
