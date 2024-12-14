using System;

namespace EncosyTower.Modules.Types.Caches
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class CacheTypesDerivedFromThisTypeAttribute : Attribute
    {
        public string[] AssemblyNames { get; }

        public CacheTypesDerivedFromThisTypeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CacheTypesWithThisAttributeAttribute : Attribute
    {
        public string[] AssemblyNames { get; }

        public CacheTypesWithThisAttributeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CacheFieldsWithThisAttributeAttribute : Attribute
    {
        public string[] AssemblyNames { get; }

        public CacheFieldsWithThisAttributeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CacheMethodsWithThisAttributeAttribute : Attribute
    {
        public string[] AssemblyNames { get; }

        public CacheMethodsWithThisAttributeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheTypesDerivedFromAttribute : Attribute
    {
        public Type BaseType { get; }

        public string[] AssemblyNames { get; }

        public CacheTypesDerivedFromAttribute(Type baseType, params string[] assemblyNames)
        {
            BaseType = baseType;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheTypesWithAttributeAttribute : Attribute
    {
        public Type AttributeType { get; }

        public string[] AssemblyNames { get; }

        public CacheTypesWithAttributeAttribute(Type attributeType, params string[] assemblyNames)
        {
            AttributeType = attributeType;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheFieldsWithAttributeAttribute : Attribute
    {
        public Type AttributeType { get; }

        public string[] AssemblyNames { get; }

        public CacheFieldsWithAttributeAttribute(Type attributeType, params string[] assemblyNames)
        {
            AttributeType = attributeType;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheMethodsWithAttributeAttribute : Attribute
    {
        public Type AttributeType { get; }

        public string[] AssemblyNames { get; }

        public CacheMethodsWithAttributeAttribute(Type attributeType, params string[] assemblyNames)
        {
            AttributeType = attributeType;
            AssemblyNames = assemblyNames;
        }
    }
}
