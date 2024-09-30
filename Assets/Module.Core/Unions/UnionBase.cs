using System.Runtime.InteropServices;

namespace Module.Core.Unions
{
    /// <summary>
    /// Represents a base layout for the <see cref="Union"/> type.
    /// <br/>
    /// The first 8-bytes block stores the metadata.
    /// <br/>
    /// The second 8-bytes block stores object reference.
    /// <br/>
    /// The rest stores other data.
    /// </summary>
    /// <seealso cref="UnionData"/>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct UnionBase
    {
        public const int META_OFFSET = 0;
        public const int META_SIZE = sizeof(ulong);
        public const int OBJECT_OFFSET = META_OFFSET + 8;
        public const int DATA_OFFSET = META_OFFSET + META_SIZE + OBJECT_OFFSET;

        [FieldOffset(META_OFFSET)] public readonly ulong Meta;
        // !!! OBJECT_OFFSET will go here !!!
        [FieldOffset(DATA_OFFSET)] public readonly UnionData Data;

        public UnionBase(ulong meta, in UnionData data)
        {
            Meta = meta;
            Data = data;
        }
    }
}
