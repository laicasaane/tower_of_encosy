using System;
using System.ComponentModel;
using System.Globalization;
using EncosyTower.Conversion;

namespace EncosyTower.Serialization
{
    public abstract class ParsableStructConverter<T> : TypeConverter
        where T : struct, ITryParse<T>
    {
        public virtual bool IgnoreCase => true;

        public virtual bool AllowMatchingMetadataAttribute => true;

        public sealed override bool CanConvertFrom(
              ITypeDescriptorContext context
            , Type sourceType
        )
        {
            return sourceType == typeof(string);
        }

        public sealed override object ConvertFrom(
              ITypeDescriptorContext context
            , CultureInfo culture
            , object value
        )
        {
            return value is string str
                ? (default(T).TryParse(str, out var result, IgnoreCase, AllowMatchingMetadataAttribute) ? result : default)
                : value;
        }
    }
}
