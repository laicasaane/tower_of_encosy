using System.Runtime.InteropServices;

namespace EncosyTower.Variants
{
    /// <summary>
    /// Represents a base layout for the <see cref="Variant"/> type.
    /// </summary>
    /// <seealso cref="VariantData"/>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct VariantBase
    {
        public const int META_OFFSET = 0;
        public const int META_SIZE = sizeof(ulong);
        public const int OBJECT_OFFSET = META_OFFSET + 8;
        public const int DATA_OFFSET = META_OFFSET + META_SIZE + OBJECT_OFFSET;

        [FieldOffset(META_OFFSET)] public readonly ulong Meta;

        // !!! OBJECT_OFFSET will go here !!!

        /// <summary>
        /// By default, this field can store up to 16 bytes of data.
        /// To increase its capacity, follow the instruction at <see cref="VariantData"/>.
        /// </summary>
        [FieldOffset(DATA_OFFSET)] public readonly VariantData Data;

        public VariantBase(ulong meta, in VariantData data)
        {
            Meta = meta;
            Data = data;
        }
    }
}
