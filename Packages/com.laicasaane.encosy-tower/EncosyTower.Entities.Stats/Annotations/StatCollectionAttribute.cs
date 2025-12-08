using System;

namespace EncosyTower.Entities.Stats
{
    /// <summary>
    /// Annotates any struct to generate functionality related to stat collection for a specific stat system.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Requires nested structs that are annotated with <see cref="StatDataAttribute"/>.</item>
    /// <item>Requires any struct or class that is annotated with <see cref="StatSystemAttribute"/>.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [StatCollection(typeof(StatSystem))]
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
        public StatCollectionAttribute(Type statSystemType)
        {
            StatSystemType = statSystemType;
        }

        public Type StatSystemType { get; }
    }
}
