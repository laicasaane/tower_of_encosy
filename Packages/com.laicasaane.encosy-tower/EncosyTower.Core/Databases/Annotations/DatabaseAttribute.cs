using System;
using EncosyTower.Naming;

namespace EncosyTower.Databases
{
    /// <summary>
    /// Marks a class or struct as a database definition that uses the BakingSheet pipeline
    /// to import data from external sources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For source generation to work, the annotated type must declare one or more properties
    /// annotated with <see cref="TableAttribute"/>.
    /// </para>
    /// <para>
    /// By default, generated table collections use <see cref="Cathei.BakingSheet.VerticalList{T}"/>.
    /// To override this behavior, apply <see cref="Authoring.HorizontalAttribute"/> to indicate
    /// that the target property should use a horizontal list.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class DatabaseAttribute : Attribute
    {
        public NamingStrategy NamingStrategy { get; }

        public Type[] Converters { get; }

        public string AssetName { get; set; }

        public bool WithInstanceAPI { get; set; }

        public DatabaseAttribute(params Type[] converters)
        {
            Converters = converters ?? Array.Empty<Type>();
        }

        public DatabaseAttribute(NamingStrategy namingStrategy, params Type[] converters)
        {
            NamingStrategy = namingStrategy;
            Converters = converters ?? Array.Empty<Type>();
        }
    }
}
