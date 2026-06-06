using System;

namespace EncosyTower.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DataPropertyAttribute : Attribute
    {
        public DataPropertyAttribute()
        {
        }

        public DataPropertyAttribute(Type fieldType, Type converterType = null)
        {
            FieldType = fieldType;
            ConverterType = converterType;
        }

        public Type FieldType { get; }

        public Type ConverterType { get; }
    }
}
