using System;

namespace EncosyTower.Data
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public Type Type { get; }

        public PropertyTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
