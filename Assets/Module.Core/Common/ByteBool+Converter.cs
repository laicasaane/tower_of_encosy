using System.ComponentModel;
using Module.Core.Serialization;

namespace Module.Core
{
    [TypeConverter(typeof(TypeConverter))]
    partial struct ByteBool
    {
        public sealed class TypeConverter : ParsableStructConverter<ByteBool>
        {
        }
    }
}
