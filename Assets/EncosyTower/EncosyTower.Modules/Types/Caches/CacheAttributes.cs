using System;

namespace EncosyTower.Modules.Types.Caches
{
    public interface ICacheAttributeWithType
    {
        Type Type { get; }
    }

    public interface ICacheAttributeWithAssemblyNames
    {
        string[] AssemblyNames { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class CacheTypesDerivedFromThisTypeAttribute : Attribute
        , ICacheAttributeWithAssemblyNames
    {
        public string[] AssemblyNames { get; }

        public CacheTypesDerivedFromThisTypeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CacheTypesWithThisAttributeAttribute : Attribute
        , ICacheAttributeWithAssemblyNames
    {
        public string[] AssemblyNames { get; }

        public CacheTypesWithThisAttributeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CacheFieldsWithThisAttributeAttribute : Attribute
        , ICacheAttributeWithAssemblyNames
    {
        public string[] AssemblyNames { get; }

        public CacheFieldsWithThisAttributeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CacheMethodsWithThisAttributeAttribute : Attribute
        , ICacheAttributeWithAssemblyNames
    {
        public string[] AssemblyNames { get; }

        public CacheMethodsWithThisAttributeAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheTypesDerivedFromAttribute : Attribute
        , ICacheAttributeWithType
        , ICacheAttributeWithAssemblyNames
    {
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheTypesDerivedFromAttribute(Type baseType, params string[] assemblyNames)
        {
            Type = baseType;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheTypesWithAttributeAttribute : Attribute
        , ICacheAttributeWithType
        , ICacheAttributeWithAssemblyNames
    {
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheTypesWithAttributeAttribute(Type attributeType, params string[] assemblyNames)
        {
            Type = attributeType;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheFieldsWithAttributeAttribute : Attribute
        , ICacheAttributeWithType
        , ICacheAttributeWithAssemblyNames
    {
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheFieldsWithAttributeAttribute(Type attributeType, params string[] assemblyNames)
        {
            Type = attributeType;
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheMethodsWithAttributeAttribute : Attribute
        , ICacheAttributeWithType
        , ICacheAttributeWithAssemblyNames
    {
        public Type Type { get; }

        public string[] AssemblyNames { get; }

        public CacheMethodsWithAttributeAttribute(Type attributeType, params string[] assemblyNames)
        {
            Type = attributeType;
            AssemblyNames = assemblyNames;
        }
    }
}
