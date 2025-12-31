using System;

namespace EncosyTower.Types.Caches
{
    public interface ICacheAttributeWithType
    {
        Type Type { get; }
    }

    public interface ICacheAttributeWithAssemblyNames
    {
        string[] AssemblyNames { get; }
    }

    /// <summary>
    /// Signals the RuntimeTypeCache system to cache the information on any type when it is annotated
    /// with this attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheTypeAttribute"/> in case the type source code is open
    /// to direct modifications.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum
        | AttributeTargets.Interface
        | AttributeTargets.Delegate
        , AllowMultiple = false
        , Inherited = false
    )]
    public sealed class CacheThisTypeAttribute : Attribute
    {
    }

    /// <summary>
    /// Signals the RuntimeTypeCache system to cache the information on a type and its descendants when it is
    /// annotated with this attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheTypesDerivedFromAttribute"/> in case the type source code
    /// is open to direct modifications.
    /// </remarks>
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

    /// <summary>
    /// Annotates any attribute to signal the RuntimeTypeCache system to cache to information on any type that is,
    /// in turn, annotated with that attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheTypesWithAttributeAttribute"/> in case
    /// the attribute source code is open to direct modifications.
    /// </remarks>
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

    /// <summary>
    /// Annotates any attribute to signal the RuntimeTypeCache system to cache to information on any field that is,
    /// in turn, annotated with that attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheFieldsWithAttributeAttribute"/> in case
    /// the attribute source code is open to direct modifications.
    /// </remarks>
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

    /// <summary>
    /// Annotates any attribute to signal the RuntimeTypeCache system to cache to information on any method that is,
    /// in turn, annotated with that attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheMethodsWithAttributeAttribute"/> in case
    /// the attribute source code is open to direct modifications.
    /// </remarks>
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

    /// <summary>
    /// Signals the RuntimeTypeCache system to cache the information on a specific type.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheThisTypeAttribute"/> in case the type source code
    /// is closed to direct modifications.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CacheTypeAttribute : Attribute
        , ICacheAttributeWithType
    {
        public Type Type { get; }

        public CacheTypeAttribute(Type baseType)
        {
            Type = baseType;
        }
    }

    /// <summary>
    /// Signals the RuntimeTypeCache system to cache the information on a specific type and its descendants.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheTypesDerivedFromThisTypeAttribute"/> in case
    /// the type source code is closed to direct modifications.
    /// </remarks>
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

    /// <summary>
    /// Signals the RuntimeTypeCache system to cache the information on types annotated with a specific attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheTypesDerivedFromThisTypeAttribute"/> in case
    /// the type source code is closed to direct modifications.
    /// </remarks>
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
    /// <summary>
    /// Signals the RuntimeTypeCache system to cache the information on fields annotated with a specific attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheTypesDerivedFromThisTypeAttribute"/> in case
    /// the type source code is closed to direct modifications.
    /// </remarks>

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

    /// <summary>
    /// Signals the RuntimeTypeCache system to cache the information on methods annotated with a specific attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is an alternative of <see cref="CacheTypesDerivedFromThisTypeAttribute"/> in case
    /// the type source code is closed to direct modifications.
    /// </remarks>
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
