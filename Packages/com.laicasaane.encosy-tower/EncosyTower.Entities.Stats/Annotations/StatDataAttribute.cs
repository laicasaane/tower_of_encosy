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

        [Obsolete("This usecase has yet fully explored.")]
        private bool WithIndex { get; set; }
    }
}
