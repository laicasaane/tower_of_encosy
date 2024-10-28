using System.ComponentModel;
using EncosyTower.Modules.Serialization;

namespace EncosyTower.Modules
{
    [TypeConverter(typeof(TypeConverter))]
    partial struct ByteBool
    {
        public sealed class TypeConverter : ParsableStructConverter<ByteBool>
        {
        }
    }
}
