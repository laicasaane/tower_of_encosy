using System;

namespace Module.Core.TypeWrap.SourceGen
{
    [Flags]
    public enum SpecialMethodType
    {
        None        = 0,
        GetHashCode = 1 << 0,
        ToString    = 1 << 1,
    }
}
