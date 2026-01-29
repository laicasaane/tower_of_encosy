using System;

namespace EncosyTower.SourceGen
{
    [Flags]
    public enum ParsableStructConverterSettings : byte
    {
        None = 0,
        IgnoreCase = 1 << 0,
        AllowMatchingMetadataAttribute = 1 << 1,
    }
}
