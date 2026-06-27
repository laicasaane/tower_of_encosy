using System;

namespace EncosyTower.Databases.Authoring
{
    /// <summary>
    /// Marks a type as a BakingSheet authoring database definition.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The authoring pipeline requires a type annotated with <see cref="AuthorDatabaseAttribute"/> so that
    /// source generation can produce the code needed to convert BakingSheet data into the runtime database format.
    /// </para>
    /// <para>
    /// This attribute links the authoring type to its runtime database type declared with
    /// <see cref="EncosyTower.Databases.DatabaseAttribute"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [AuthorDatabase(typeof(SampleDatabase))]
    /// public readonly partial struct SampleDatabaseAuthoring { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class AuthorDatabaseAttribute : Attribute
    {
        /// <summary>
        /// Gets the runtime database type associated with this authoring type.
        /// </summary>
        public Type DatabaseType { get; }

        /// <remarks>
        /// Converter precedences (smaller index means higher precedence):
        /// <list type="number">
        /// <item><see cref="EncosyTower.Databases.Authoring.ConverterForDataPropertyAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.Authoring.ConverterForTableAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.Authoring.AuthorDatabaseAttribute"/></item>
        /// <item><see cref="EncosyTower.Data.Authoring.DataAuthoringConverterAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.TableAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.DatabaseAttribute"/></item>
        /// <item>Local <c>Convert</c> method within IData type</item>
        /// </list>
        /// </remarks>
        public Type[] Converters { get; }

        /// <summary>
        /// When set to <c>true</c>, source generator will output sheet names based on
        /// fully qualified data type.
        /// <br/>
        /// Default to <c>false</c>.
        /// </summary>
        public bool FullyQualifiedSheetNames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorDatabaseAttribute"/> class.
        /// </summary>
        /// <param name="databaseType">The runtime database type to bind to this authoring type.</param>
        /// <param name="converters">The converter types used by this database.</param>
        public AuthorDatabaseAttribute(Type databaseType, params Type[] converters)
        {
            DatabaseType = databaseType;
            Converters = converters;
        }
    }
}
