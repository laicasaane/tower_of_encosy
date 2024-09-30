#if UNITY_ENTITIES

using System;

namespace Module.Core.Entities
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
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
