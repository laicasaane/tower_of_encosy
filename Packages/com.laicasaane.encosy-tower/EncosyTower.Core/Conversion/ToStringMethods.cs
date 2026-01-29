using System;

namespace EncosyTower.Conversion
{
    [Flags]
    public enum ToStringMethods : byte
    {
        Default              = 0,
        ToDisplayString      = 1 << 0,
#if UNITY_COLLECTIONS
        ToFixedString        = 1 << 1,
        ToDisplayFixedString = 1 << 2,
        All                  = ToDisplayString | ToFixedString | ToDisplayFixedString,
#else
        All                  = ToDisplayString,
#endif
    }
}
