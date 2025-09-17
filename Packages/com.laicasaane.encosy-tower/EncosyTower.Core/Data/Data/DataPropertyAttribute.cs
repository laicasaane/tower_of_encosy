using System;

namespace EncosyTower.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DataPropertyAttribute : Attribute
    {
        public DataPropertyAttribute()
        {
        }

        public DataPropertyAttribute(Type fieldType)
        {
            FieldType = fieldType;
        }

        public Type FieldType { get; }
    }
}
