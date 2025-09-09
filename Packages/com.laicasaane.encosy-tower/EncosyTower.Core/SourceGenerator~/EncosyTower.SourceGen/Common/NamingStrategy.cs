﻿namespace Newtonsoft.Json.Utilities
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

    public static class NamingStrategyExtensions
    {
        public static NamingStrategy ToNamingStrategy(this object value)
        {
            if (value is not int intValue)
                return NamingStrategy.PascalCase;

            return (NamingStrategy)intValue;
        }

        public static string ToNamingCase(this string value, NamingStrategy strategy)
            => strategy switch {
                NamingStrategy.CamelCase => StringUtils.ToCamelCase(value),
                NamingStrategy.SnakeCase => StringUtils.ToSnakeCase(value),
                NamingStrategy.KebabCase => StringUtils.ToKebabCase(value),
                _ => value,
            };
    }
}
