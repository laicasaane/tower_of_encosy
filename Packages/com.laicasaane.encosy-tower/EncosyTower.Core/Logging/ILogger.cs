namespace EncosyTower.Logging
{
    using System;

    public partial interface ILogger
    {
        void LogException(Exception value);

        void LogInfo(object message);

        void LogInfoFormat(string format, params object[] args);

        void LogWarning(object message);

        void LogWarningFormat(string format, params object[] args);

        void LogError(object message);

        void LogErrorFormat(string format, params object[] args);
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Logging
{
    using Unity.Collections;

    partial interface ILogger
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
