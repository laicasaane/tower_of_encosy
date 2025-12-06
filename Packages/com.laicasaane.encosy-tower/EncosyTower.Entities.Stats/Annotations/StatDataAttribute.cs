using System;

namespace EncosyTower.Entities.Stats
{
    /// <summary>
    /// Annotates any struct to generate functionality related to stat data.
    /// </summary>
    /// <remarks>
    /// The struct will automatically implement <see cref="IStatData"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// [StatData(StatVariantType.Float)]
    /// public partial struct Hp { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class StatDataAttribute : Attribute
    {
        public StatDataAttribute(StatVariantType valueType)
        {
            ValueType = valueType;
        }

        public StatDataAttribute(Type enumType)
        {
            EnumType = enumType;
        }

        public StatVariantType ValueType { get; }

        public Type EnumType { get; }

        public bool SingleValue { get; set; }
    }
}
