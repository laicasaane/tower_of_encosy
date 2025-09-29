#if !UNITY_COLLECTIONS

using System;
using System.Runtime.InteropServices;

namespace Unity.Collections
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct FixedBytes16
    {
        [FieldOffset(00)] public byte byte0000;
        [FieldOffset(01)] public byte byte0001;
        [FieldOffset(02)] public byte byte0002;
        [FieldOffset(03)] public byte byte0003;
        [FieldOffset(04)] public byte byte0004;
        [FieldOffset(05)] public byte byte0005;
        [FieldOffset(06)] public byte byte0006;
        [FieldOffset(07)] public byte byte0007;
        [FieldOffset(08)] public byte byte0008;
        [FieldOffset(09)] public byte byte0009;
        [FieldOffset(10)] public byte byte0010;
        [FieldOffset(11)] public byte byte0011;
        [FieldOffset(12)] public byte byte0012;
        [FieldOffset(13)] public byte byte0013;
        [FieldOffset(14)] public byte byte0014;
        [FieldOffset(15)] public byte byte0015;
    }
}

#endif
