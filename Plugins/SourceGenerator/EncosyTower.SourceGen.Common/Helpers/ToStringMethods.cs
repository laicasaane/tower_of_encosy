using System;

namespace EncosyTower.SourceGen
{
    [Flags]
    public enum ToStringMethods : byte
    {
        Default              = 0,
        ToDisplayString      = 1 << 0,
        ToFixedString        = 1 << 1,
        ToDisplayFixedString = 1 << 2,
        All                  = ToDisplayString | ToFixedString | ToDisplayFixedString,
    }
}
