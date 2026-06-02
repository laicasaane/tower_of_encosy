using System.Runtime.CompilerServices;

namespace EncosyTower.Naming
{
    /// <summary>
    /// The supported formats of sheet and column names.
    /// </summary>
    public enum NameCasing
    {
        /// <summary>
        /// All words start with an uppercase character.
        /// </summary>
        /// <example>
        /// <para><b>Original name:</b> <c>tempCelsius</c></para>
        /// <para><b>Converted name:</b> <c>TempCelsius</c></para>
        /// </example>
        Pascal = 0,

        /// <summary>
        /// First word starts with a lower case character.
        /// Successive words start with an uppercase character.
        /// </summary>
        /// <example>
        /// <para><b>Original name:</b> <c>TempCelsius</c></para>
        /// <para><b>Converted name:</b> <c>tempCelsius</c></para>
        /// </example>
        Camel = 1,

        /// <summary>
        /// Words are separated by underscores. All characters are lowercase.
        /// </summary>
        /// <example>
        /// <para><b>Original name:</b> <c>TempCelsius</c></para>
        /// <para><b>Converted name:</b> <c>temp_celsius</c></para>
        /// </example>
        SnakeLower = 2,

        /// <summary>
        /// Words are separated by underscores. All characters are lowercase.
        /// </summary>
        /// <example>
        /// <para><b>Original name:</b> <c>TempCelsius</c></para>
        /// <para><b>Converted name:</b> <c>TEMP_CELSIUS</c></para>
        /// </example>
        SnakeUpper = 3,

        /// <summary>
        /// Words are separated by hyphens. All characters are lowercase.
        /// </summary>
        /// <example>
        /// <para><b>Original name:</b> <c>TempCelsius</c></para>
        /// <para><b>Converted name:</b> <c>temp-celsius</c></para>
        /// </example>
        KebabLower = 4,

        /// <summary>
        /// Words are separated by hyphens. All characters are uppercase.
        /// </summary>
        /// <example>
        /// <para><b>Original name:</b> <c>TempCelsius</c></para>
        /// <para><b>Converted name:</b> <c>TEMP-CELSIUS</c></para>
        /// </example>
        KebabUpper = 5,
    }

    public static class NameCasingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ConvertName(this NameCasing self, string name)
            => ToPolicy(self).ConvertName(name);

        public static NamingPolicy ToPolicy(this NameCasing self)
            => self switch {
                NameCasing.Camel => NamingPolicy.CamelCase,
                NameCasing.SnakeLower => NamingPolicy.SnakeCaseLower,
                NameCasing.SnakeUpper => NamingPolicy.SnakeCaseUpper,
                NameCasing.KebabLower => NamingPolicy.KebabCaseLower,
                NameCasing.KebabUpper => NamingPolicy.KebabCaseUpper,
                _ => NamingPolicy.PascalCase,
            };
    }
}
