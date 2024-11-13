#if UNITY_ENTITIES

using System;

namespace EncosyTower.Modules.Entities
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class LookupAttribute : Attribute
    {
        public Type Type { get; }

        public bool IsReadOnly { get; }

        public LookupAttribute(Type type)
        {
            Type = type;
            IsReadOnly = false;
        }

        public LookupAttribute(Type type, bool isReadOnly)
        {
            Type = type;
            IsReadOnly = isReadOnly;
        }
    }
}

#endif
