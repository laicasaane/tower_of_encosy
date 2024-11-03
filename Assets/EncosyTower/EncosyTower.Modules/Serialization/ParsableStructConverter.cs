using System;
using System.ComponentModel;
using System.Globalization;

namespace EncosyTower.Modules.Serialization
{
    public abstract class ParsableStructConverter<T> : TypeConverter
        where T : struct, ITryParse<T>
    {
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
            var defaultValue = default(T);
            return value is string str ? (defaultValue.TryParse(str, out var result) ? result : defaultValue) : value;
        }
    }
}
