namespace EncosyTower.Entities.Stats
{
    public interface IStatValuePair
    {
        StatVariantType Type { get; }

        bool IsPair { get; }

        StatVariant GetBaseValueOrDefault(in StatVariant defaultValue = default);

        StatVariant GetCurrentValueOrDefault(in StatVariant defaultValue = default);

        bool TrySetBaseValue(in StatVariant value);

        bool TrySetCurrentValue(in StatVariant value);
    }

    public interface IStatValuePairComposer<TValuePair>
        where TValuePair : IStatValuePair
    {
        TValuePair Compose(bool isPar, in StatVariant baseValue, in StatVariant currentValue);
    }
}
