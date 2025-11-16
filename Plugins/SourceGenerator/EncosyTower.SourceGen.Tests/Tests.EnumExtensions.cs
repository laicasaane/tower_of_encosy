using System;
using EncosyTower.EnumExtensions;

namespace EncosyTower.Tests.EnumExtensions
{
    [EnumExtensions]
    public enum FruitType : byte { Apple, Orange, }

    partial class FruitTypeExtensions { }

    [EnumExtensionsFor(typeof(System.DayOfWeek))]
    public static partial class DayOfWeekExtensions { }

    [Flags, EnumExtensions]
    public enum BitFlags
    {
        None = 0,
        Bit1 = 1 << 0,
        Bit2 = 2 << 0,
        Bit3 = 3 << 0,
    }

    partial class BitFlagsExtensions { }
}
