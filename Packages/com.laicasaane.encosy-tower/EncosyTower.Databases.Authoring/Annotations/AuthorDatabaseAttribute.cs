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

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorDatabaseAttribute"/> class.
        /// </summary>
        /// <param name="databaseType">
        /// The runtime database type to bind to this authoring type.
        /// </param>
        public AuthorDatabaseAttribute(Type databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}
