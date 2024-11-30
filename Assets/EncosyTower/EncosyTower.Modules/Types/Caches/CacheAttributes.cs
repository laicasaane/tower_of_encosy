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
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheTypesDerivedFromAttribute(Type type, params string[] assemblyNames)
        {
            Type = type;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheTypesWithAttributeAttribute : Attribute
    {
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheTypesWithAttributeAttribute(Type type, params string[] assemblyNames)
        {
            Type = type;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheFieldsWithAttributeAttribute : Attribute
    {
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheFieldsWithAttributeAttribute(Type type, params string[] assemblyNames)
        {
            Type = type;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheMethodsWithAttributeAttribute : Attribute
    {
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheMethodsWithAttributeAttribute(Type type, params string[] assemblyNames)
        {
            Type = type;
            AssemblyNames = assemblyNames;
        }
    }
}
