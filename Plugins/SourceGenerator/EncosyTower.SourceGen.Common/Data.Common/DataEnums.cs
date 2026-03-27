using System;

namespace EncosyTower.SourceGen.Common.Data.Common
{
    public enum DataFieldPolicy
    {
        Private = 0,
        Internal,
        Public,
    }

    [Flags]
    public enum DataMutableOptions
    {
        Default = 0,
        WithoutPropertySetters = 1 << 0,
        WithReadOnlyStruct = 1 << 1,
    }
}
