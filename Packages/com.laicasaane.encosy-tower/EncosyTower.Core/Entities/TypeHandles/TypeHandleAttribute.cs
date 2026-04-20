#if UNITY_ENTITIES

using System;

namespace EncosyTower.Entities
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class TypeHandleAttribute : Attribute
    {
        public Type Type { get; }

        public bool IsReadOnly { get; }

        public TypeHandleAttribute(Type type)
        {
            Type = type;
            IsReadOnly = false;
        }

        public TypeHandleAttribute(Type type, bool isReadOnly)
        {
            Type = type;
            IsReadOnly = isReadOnly;
        }
    }
}

#endif
