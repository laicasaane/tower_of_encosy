using UnityEngine;
using UnityEngine.Scripting;

namespace EncosyTower.Modules
{
    [Preserve]
    public static class BuiltInTypeIds
    {
        public static readonly TypeId Bool;
        public static readonly TypeId Byte;
        public static readonly TypeId SByte;
        public static readonly TypeId Char;
        public static readonly TypeId Double;
        public static readonly TypeId Float;
        public static readonly TypeId Int;
        public static readonly TypeId UInt;
        public static readonly TypeId Long;
        public static readonly TypeId ULong;
        public static readonly TypeId Short;
        public static readonly TypeId UShort;
        public static readonly TypeId String;
        public static readonly TypeId Object;

        [Preserve]
        static BuiltInTypeIds()
        {
            Bool = TypeId.Get<bool>();
            Byte = TypeId.Get<byte>();
            SByte = TypeId.Get<sbyte>();
            Char = TypeId.Get<char>();
            Double = TypeId.Get<double>();
            Float = TypeId.Get<float>();
            Int = TypeId.Get<int>();
            UInt = TypeId.Get<uint>();
            Long = TypeId.Get<long>();
            ULong = TypeId.Get<ulong>();
            Short = TypeId.Get<short>();
            UShort = TypeId.Get<ushort>();
            String = TypeId.Get<string>();
            Object = TypeId.Get<object>();
        }

        [Preserve]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitOnLoad() { }
    }
}
