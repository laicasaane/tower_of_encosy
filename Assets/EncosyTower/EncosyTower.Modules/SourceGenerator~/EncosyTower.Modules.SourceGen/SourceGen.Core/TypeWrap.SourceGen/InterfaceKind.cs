using System;

namespace EncosyTower.Modules.TypeWrap.SourceGen
{
    [Flags]
    public enum InterfaceKind
    {
        None       = 0,
        Equatable  = 1 << 0,
        Comparable = 1 << 1,
    }
}
