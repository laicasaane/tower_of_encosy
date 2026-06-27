using System;

namespace EncosyTower.Databases.Authoring
{
    /// <summary>
    /// Annotates the type which is annotated with <see cref="AuthorDatabaseAttribute"/>
    /// to specify a converter for a specific type used within a table.
    /// </summary>
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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ConverterForTableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterForTableAttribute"/> class
        /// with a specified table name.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table which sets the context for the converter.
        /// The table must be correctly declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </param>
        /// <param name="converterType">The type of the converter.</param>
        public ConverterForTableAttribute(string tableName, Type converterType)
        {
            TableName = tableName;
            ConverterType = converterType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterForTableAttribute"/> class
        /// with a specified table type.
        /// </summary>
        /// <param name="tableType">
        /// The type of the table which sets the context for the converter.
        /// The table type must be the return type of one or multiple tables
        /// correctly declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </param>
        /// <param name="converterType">The type of the converter.</param>
        public ConverterForTableAttribute(Type tableType, Type converterType)
        {
            TableType = tableType;
            ConverterType = converterType;
        }

        /// <summary>
        /// Gets the name of the table which sets the context for the converter.
        /// The table must be correctly declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </summary>
        /// <remarks>
        /// The specified table acts as the context for the converter
        /// to determine which converter to use when multiple converters are specified
        /// for the same type.
        /// </remarks>
        public string TableName { get; }

        /// <summary>
        /// Gets the type of the table which sets the context for the converter.
        /// The table type must be the return type of one or multiple tables
        /// correctly declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </summary>
        /// <remarks>
        /// The specified table acts as the context for the converter
        /// to determine which converter to use when multiple converters are specified
        /// for the same type.
        /// </remarks>
        public Type TableType { get; }

        /// <summary>
        /// Gets the type of the converter.
        /// </summary>
        public Type ConverterType { get; }
    }
}
