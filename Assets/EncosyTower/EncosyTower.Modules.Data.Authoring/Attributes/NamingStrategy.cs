namespace EncosyTower.Modules.Data.Authoring
{
    /// <summary>
    /// The supported formats of sheet and column names.
    /// </summary>
    public enum NamingStrategy
    {
        /// <summary>
        /// To support column names in the format of "ColumnName".
        /// </summary>
        PascalCase = 0,

        /// <summary>
        /// To support column names in the format of "columnName".
        /// </summary>
        CamelCase = 1,

        /// <summary>
        /// To support column names in the format of "column_name".
        /// </summary>
        SnakeCase = 2,

        /// <summary>
        /// To support column names in the format of "column-name".
        /// </summary>
        KebabCase = 3,
    }
}
