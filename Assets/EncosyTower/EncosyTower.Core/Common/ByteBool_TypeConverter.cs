using System.ComponentModel;
using EncosyTower.Serialization;

namespace EncosyTower.Common
{
    [TypeConverter(typeof(TypeConverter))]
    partial struct ByteBool
    {
        public sealed class TypeConverter : ParsableStructConverter<ByteBool>
        {
        }
    }
}
