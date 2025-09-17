namespace EncosyTower.Logging
{
    using System;

    public partial interface ILogger
    {
        CallerInfo GetCallerInfo(
              int lineNumber = 0
            , string memberName = ""
            , string filePath = ""
        );

        void LogException(Exception value);

        void LogInfo(object message);

        void LogInfoFormat(string format, params object[] args);

        void LogWarning(object message);

        void LogWarningFormat(string format, params object[] args);

        void LogError(object message);

        void LogErrorFormat(string format, params object[] args);

        void LogInfoSlim(object message);

        void LogInfoFormatSlim(string format, params object[] args);

        void LogWarningSlim(object message);

        void LogWarningFormatSlim(string format, params object[] args);

        void LogErrorSlim(object message);

        void LogErrorFormatSlim(string format, params object[] args);
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
