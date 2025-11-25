namespace EncosyTower.Entities.Stats
{
    public interface IStatData
    {
        bool IsValuePair { get; }
        
        StatVariantType ValueType { get; }

        StatVariant BaseValue { get; set; }

        StatVariant CurrentValue { get; set; }
    }
}
