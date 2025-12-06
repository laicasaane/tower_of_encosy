using System;

namespace EncosyTower.Entities.Stats
{
    /// <summary>
    /// Annotates any struct to generate functionality related to stat collection.
    /// </summary>
    /// <remarks>
    /// Requires nested structs annotated with <see cref="StatDataAttribute"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// [StatCollection]
    /// public partial struct Stats
    /// {
    ///     [StatData(StatVariantType.Float)]
    ///     public struct Hp { }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class StatCollectionAttribute : Attribute
    {
    }
}
