using System;
using EncosyTower.EnumExtensions;

namespace EncosyTower.Pooling.Native
{
    [Flags, EnumExtensions]
    public enum NativeReturningOptions : byte
    {
        None        = 0,
        Deactivate  = 1 << 0,
        MoveToScene = 1 << 1,
        Everything  = Deactivate | MoveToScene,
    }
}
