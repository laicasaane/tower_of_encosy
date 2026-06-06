using System;

namespace EncosyTower.Data
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public PropertyTypeAttribute(Type type, Type converterType = null)
        {
            Type = type;
            ConverterType = converterType;
        }

        public Type Type { get; }

        public Type ConverterType { get; }
    }
}
