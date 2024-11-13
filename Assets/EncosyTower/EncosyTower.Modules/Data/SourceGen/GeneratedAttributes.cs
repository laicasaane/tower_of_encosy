using System;

namespace EncosyTower.Modules.Data.SourceGen
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class GeneratedPropertyFromFieldAttribute : Attribute
    {
        public string FieldName { get; }

        public Type FieldType { get; }

        public GeneratedPropertyFromFieldAttribute(string fieldName, Type fieldType)
        {
            this.FieldName = fieldName;
            this.FieldType = fieldType;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GeneratedFieldFromPropertyAttribute : Attribute
    {
        public string PropertyName { get; }

        public GeneratedFieldFromPropertyAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }
    }
}
