#if UNITY_EDITOR || ENCOSY_INCLUDE_AUTHORING

using System;
using System.Globalization;
using EncosyTower.Core;


namespace EncosyTower.Data.Authoring
{
    [ApiForAuthoring]
    public readonly struct DataConvertingContext
    {
        [ApiForAuthoring]
        public static readonly DataConvertingContext Default = default;

        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;

        public IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
    }
}

#endif
