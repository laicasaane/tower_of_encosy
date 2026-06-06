using System;
using System.Globalization;
using EncosyTower.Core;


namespace EncosyTower.Data.Authoring
{
    [ApiForAuthoring]
    public readonly struct DataConvertingContext
    {
        public static readonly DataConvertingContext Default = default;

        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;

        public IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
    }
}
