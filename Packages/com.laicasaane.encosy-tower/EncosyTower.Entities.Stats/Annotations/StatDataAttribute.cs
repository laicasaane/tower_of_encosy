using System;

namespace EncosyTower.Entities.Stats
{
    /// <summary>
    /// Annotates any struct with to generate stat-related functionality.
    /// </summary>
    /// <remarks>
    /// The struct will automatically implement <see cref="IStatData"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class StatDataAttribute : Attribute
    {
        public StatDataAttribute(StatVariantType valueType, bool singleValue = false)
        {
            ValueType = valueType;
            SingleValue = singleValue;
        }
        
        public StatDataAttribute(Type enumType, bool singleValue = false)
        {
            EnumType = enumType;
            SingleValue = singleValue;
        }

        public StatVariantType ValueType { get; }

        public Type EnumType { get; }

        public bool SingleValue { get; }
    }
}
