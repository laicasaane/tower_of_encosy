#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.EnumExtensions;

namespace EncosyTower.UserDataVaults
{
    [Flags, EnumExtensions]
    public enum SaveDestination : byte
    {
        None = 0,
        Device = 1 << 0,
        Cloud = 1 << 1,
    }
}

#endif
