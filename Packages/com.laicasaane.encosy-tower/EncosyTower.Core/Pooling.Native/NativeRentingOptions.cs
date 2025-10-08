using System;
using EncosyTower.EnumExtensions;

namespace EncosyTower.Pooling.Native
{
    [Flags, EnumExtensions]
    public enum NativeRentingOptions : byte
    {
        None        = 0,
        Activate    = 1 << 0,
        MoveToScene = 1 << 1,
        Everything  = Activate | MoveToScene,
    }
}
