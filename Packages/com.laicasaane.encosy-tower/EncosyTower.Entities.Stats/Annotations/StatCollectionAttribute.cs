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
    /// <item>
    /// By default, TypeId values will be casted to Stat.UserData as-is. If there are multiple stat collections,
    /// it is desirable to offset the TypeId of each collection so their Stat.UserData values won't overlap.
    /// In this case, use the constructor which also takes in a TypeId osset value.
    /// The sum of <see cref="TypeIdOffset"/> and the number of TypeId values
    /// should not exceed <see cref="uint.MaxValue"/>.
    /// </item>
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
        /// <summary>
        /// Specify the target stat system.
        /// </summary>
        /// <param name="statSystemType">
        /// The type of the target stat system, which is annotated with <see cref="StatSystemAttribute"/>.
        /// </param>
        public StatCollectionAttribute(Type statSystemType)
        {
            StatSystemType = statSystemType;
        }

        /// <summary>
        /// Specify the target stat system and an offset for TypeId values when they are casted to Stat.UserData.
        /// </summary>
        /// <param name="statSystemType">
        /// The type of the target stat system, which is annotated with <see cref="StatSystemAttribute"/>.
        /// </param>
        /// <param name="typeIdOffset">
        /// The offset for TypeId values when they are casted to Stat.UserData.
        /// The sum of this value and the number of TypeId values should not exceed <see cref="uint.MaxValue"/>.
        /// </param>
        public StatCollectionAttribute(Type statSystemType, uint typeIdOffset)
        {
            StatSystemType = statSystemType;
            TypeIdOffset = typeIdOffset;
        }

        /// <summary>
        /// The type of the target stat system, which is annotated with <see cref="StatSystemAttribute"/>.
        /// </summary>
        public Type StatSystemType { get; }

        /// <summary>
        /// The offset for TypeId values when they are casted to Stat.UserData.
        /// </summary>
        /// <remarks>
        /// The sum of this value and the number of TypeId values should not exceed <see cref="uint.MaxValue"/>.
        /// </remarks>
        public uint TypeIdOffset { get; }
    }
}
