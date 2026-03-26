using System;

namespace EncosyTower.Mvvm.ViewBinding
{
    /// <summary>
    /// Specifies that a class provides an adapter between a source type and a destination type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AdapterAttribute : Attribute
    {
        public const int DEFAULT_ORDER = 1000;

        public Type SourceType { get; }

        public Type DestinationType { get; }

        public int Order { get; }

        public AdapterAttribute(Type sourceType, Type destType)
            : this(sourceType, destType, DEFAULT_ORDER)
        { }

        public AdapterAttribute(Type sourceType, Type destType, int order)
        {
            this.SourceType = sourceType;
            this.DestinationType = destType;
            this.Order = order;
        }
    }
}
