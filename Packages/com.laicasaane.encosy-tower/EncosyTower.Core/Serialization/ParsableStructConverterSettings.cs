using System;

namespace EncosyTower.Serialization
{
    [Flags]
    public enum ParsableStructConverterSettings : byte
    {
        None = 0,
        IgnoreCase = 1 << 0,
        AllowMatchingMetadataAttribute = 1 << 1,
    }
}
