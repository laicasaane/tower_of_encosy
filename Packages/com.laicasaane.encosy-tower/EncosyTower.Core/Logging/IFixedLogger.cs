#if UNITY_COLLECTIONS

using Unity.Collections;

namespace EncosyTower.Logging
{

    public interface IFixedLogger
    {
        void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes;

        void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes;

        void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes;
    }
}

#endif
