using System;

namespace EncosyTower.Modules.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DataPropertyAttribute : Attribute
    {
        public Type FieldType { get; }

        public DataPropertyAttribute()
        {
        }

        public DataPropertyAttribute(Type fieldType)
        {
            this.FieldType = fieldType;
        }
    }
}
