using System;
using System.Globalization;
using EncosyTower.Core;


namespace EncosyTower.Data
{
    [ApiForAuthoring]
    public readonly struct DataConvertingContext
    {
        public static readonly DataConvertingContext Default = default;

        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;

        public IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
    }
}
