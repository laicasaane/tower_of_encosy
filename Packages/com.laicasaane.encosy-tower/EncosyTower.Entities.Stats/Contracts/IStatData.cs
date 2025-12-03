using System;

namespace EncosyTower.Entities.Stats
{
    public interface IStatData
    {
        bool IsValuePair { get; }

        StatVariantType ValueType { get; }

        StatVariant BaseValue { get; set; }

        StatVariant CurrentValue { get; set; }
    }

    [Obsolete("This usecase has yet fully explored.")]
    internal interface IStatDataWithIndex : IStatData
    {
        StatIndex StatIndex { get; set; }
    }
}
