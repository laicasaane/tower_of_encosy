using System;

namespace EncosyTower.Entities.Stats
{
    /// <summary>
    /// Annotates any static class to generate stat-related functionality.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class StatSystemAttribute : Attribute
    {
        public StatSystemAttribute(StatDataSize maxDataSize)
        {
            MaxDataSize = maxDataSize;
            MaxUserDataSize = StatUserDataSize.Size1;
        }

        public StatSystemAttribute(StatDataSize maxDataSize, StatUserDataSize maxUserDataSize)
        {
            MaxDataSize = maxDataSize;
            MaxUserDataSize = maxUserDataSize;
        }

        /// <summary>
        /// The maximum size in bytes that can store any implementation of <see cref="IStatData"/>.
        /// </summary>
        public StatDataSize MaxDataSize { get; }

        public StatUserDataSize MaxUserDataSize { get; }
    }
}
