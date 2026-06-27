using System;

namespace EncosyTower.Databases.Authoring
{
    /// <summary>
    /// Annotates the type which is annotated with <see cref="AuthorDatabaseAttribute"/>
    /// to specify a converter for a specific property of a type
    /// marked with <see cref="EncosyTower.Data.DataAttribute"/>.
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
    public sealed class ConverterForDataPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterForDataPropertyAttribute"/> class.
        /// </summary>
        /// <param name="dataType">The type which is marked with <see cref="EncosyTower.Data.DataAttribute"/>.</param>
        /// <param name="propertyName">The property of <paramref name="dataType"/>.</param>
        /// <param name="converterType">The type of the converter.</param>
        /// <remarks>
        /// This constructor is unscoped and will be used for all tables containing the specified property.
        /// </remarks>
        public ConverterForDataPropertyAttribute(Type dataType, string propertyName, Type converterType)
        {
            DataType = dataType;
            PropertyName = propertyName;
            ConverterType = converterType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterForDataPropertyAttribute"/> class
        /// with a specified table name.
        /// </summary>
        /// <param name="dataType">The type which is marked with <see cref="EncosyTower.Data.DataAttribute"/>.</param>
        /// <param name="propertyName">The property of <paramref name="dataType"/>.</param>
        /// <param name="converterType">The type of the converter.</param>
        /// <param name="tableName">
        /// The name of the table which sets the context for the converter.
        /// The table must be declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </param>
        /// <remarks>
        /// This constructor takes precedence over the unscoped
        /// <see cref="ConverterForDataPropertyAttribute(Type, string, Type)"/>
        /// when both are specified for the same property.
        /// </remarks>
        public ConverterForDataPropertyAttribute(
              Type dataType
            , string propertyName
            , Type converterType
            , string tableName
        )
        {
            DataType = dataType;
            PropertyName = propertyName;
            ConverterType = converterType;
            TableName = tableName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterForDataPropertyAttribute"/> class
        /// with a specified table type.
        /// </summary>
        /// <param name="dataType">The type which is marked with <see cref="EncosyTower.Data.DataAttribute"/>.</param>
        /// <param name="propertyName">The property of <paramref name="dataType"/>.</param>
        /// <param name="converterType">The type of the converter.</param>
        /// <param name="tableType">
        /// The type of the table which sets the context for the converter.
        /// The table type must be the return type of a table declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </param>
        /// <remarks>
        /// This constructor takes precedence over the unscoped
        /// <see cref="ConverterForDataPropertyAttribute(Type, string, Type)"/>
        /// when both are specified for the same property.
        /// </remarks>
        public ConverterForDataPropertyAttribute(
              Type dataType
            , string propertyName
            , Type converterType
            , Type tableType
        )
        {
            DataType = dataType;
            PropertyName = propertyName;
            ConverterType = converterType;
            TableType = tableType;
        }

        /// <summary>
        /// Gets the type which is marked with <see cref="EncosyTower.Data.DataAttribute"/>
        /// that contains <see cref="PropertyName"/>.
        /// </summary>
        public Type DataType { get; }

        /// <summary>
        /// Gets the property name of <see cref="DataType"/>.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the type of the converter for the specified <see cref="DataType"/>
        /// and <see cref="PropertyName"/> combination.
        /// </summary>
        public Type ConverterType { get; }

        /// <summary>
        /// Gets the name of the table which sets the context for the converter.
        /// The table must be declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </summary>
        /// <remarks>
        /// The specified table acts as the context for the <see cref="DataType"/>
        /// and <see cref="PropertyName"/> combination to determine which converter to use
        /// when multiple converters are specified for the same <see cref="DataType"/>
        /// and <see cref="PropertyName"/>.
        /// <br/>
        /// When unspecified, the converter will be used for all <see cref="DataType"/>
        /// and <see cref="PropertyName"/> combinations across all tables within the database.
        /// </remarks>
        public string TableName { get; }

        /// <summary>
        /// Gets the type of the table which sets the context for the converter.
        /// The table type must be the return type of a table declared within the database type referenced
        /// by <see cref="AuthorDatabaseAttribute.DatabaseType"/>.
        /// </summary>
        /// <remarks>
        /// The specified table acts as the context for the <see cref="DataType"/>
        /// and <see cref="PropertyName"/> combination to determine which converter to use
        /// when multiple converters are specified for the same <see cref="DataType"/>
        /// and <see cref="PropertyName"/>.
        /// <br/>
        /// When unspecified, the converter will be used for all <see cref="DataType"/>
        /// and <see cref="PropertyName"/> combinations across all tables within the database.
        /// </remarks>
        public Type TableType { get; }
    }
}
