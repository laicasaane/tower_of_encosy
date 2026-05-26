using System;
using EncosyTower.Core;
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
    /// By default, generated table collections use <c>Cathei.BakingSheet.VerticalList&lt;T&gt;</c>.
    /// To override this behavior, apply <see cref="Authoring.HorizontalAttribute"/> to indicate
    /// that the target property should use a horizontal list.
    /// </para>
    /// </remarks>
    [ApiForAuthoring]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class DatabaseAttribute : Attribute
    {
        public NamingStrategy NamingStrategy { get; }

        /// <remarks>
        /// Converter precedences (smaller index means higher precedence):
        /// <list type="number">
        /// <item><see cref="EncosyTower.Data.DataConverterAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.Authoring.AuthorDatabaseAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.DatabaseAttribute"/></item>
        /// </list>
        /// </remarks>
        public Type[] Converters { get; }

        public string AssetName { get; set; }

        public bool WithInstanceAPI { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseAttribute"/> class.
        /// </summary>
        /// <param name="converters">The converter types used by this database.</param>
        public DatabaseAttribute(params Type[] converters)
        {
            Converters = converters ?? Array.Empty<Type>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseAttribute"/> class.
        /// </summary>
        /// <param name="namingStrategy">The naming strategy for generated database members.</param>
        /// <param name="converters">The converter types used by this database.</param>
        public DatabaseAttribute(NamingStrategy namingStrategy, params Type[] converters)
        {
            NamingStrategy = namingStrategy;
            Converters = converters ?? Array.Empty<Type>();
        }
    }
}
