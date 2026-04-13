using System;

namespace EncosyTower.SourceGen.Helpers.Data
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
