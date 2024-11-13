using System;

namespace EncosyTower.Modules.Mvvm.ViewBinding
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
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
