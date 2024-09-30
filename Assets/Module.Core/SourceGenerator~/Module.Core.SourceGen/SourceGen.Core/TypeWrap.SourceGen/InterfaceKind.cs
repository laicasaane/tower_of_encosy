using System;

namespace Module.Core.TypeWrap.SourceGen
{
    [Flags]
    public enum InterfaceKind
    {
        None       = 0,
        Equatable  = 1 << 0,
        Comparable = 1 << 1,
    }
}
