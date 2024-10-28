using System;

namespace EncosyTower.Modules.UnionIds
{
    [Flags]
    public enum UnionIdKindSettings : byte
    {
        None = 0,

        AllowEmpty = 1 << 0,

        PreserveOrder = 1 << 1,

        /// <summary>
        /// Remove "Type" and "Kind" suffixes
        /// </summary>
        RemoveSuffix = 1 << 2,
    }
}
