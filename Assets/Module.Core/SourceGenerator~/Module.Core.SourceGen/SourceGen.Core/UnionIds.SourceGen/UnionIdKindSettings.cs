using System;

namespace Module.Core.UnionIds.SourceGen
{
    [Flags]
    public enum UnionIdKindSettings : byte
    {
        None = 0,
        AllowEmpty = 1 << 0,
        PreserveOrder = 1 << 1,
        RemoveSuffix = 1 << 2,
    }
}
