namespace EncosyTower.Logging
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public partial class Logger : ILogger
    {
        public static readonly Logger Default = new();

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CallerInfo GetCallerInfo(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            return StaticLogger.GetCallerInfo(lineNumber, memberName, filePath);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            StaticLogger.LogException(value);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            StaticLogger.LogInfo(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            StaticLogger.LogInfoFormat(format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            StaticLogger.LogWarning(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            StaticLogger.LogWarningFormat(format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            StaticLogger.LogError(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            StaticLogger.LogErrorFormat(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoSlim(object message)
        {
            StaticLogger.LogInfoSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormatSlim(string format, params object[] args)
        {
            StaticLogger.LogInfoFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningSlim(object message)
        {
            StaticLogger.LogWarningSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormatSlim(string format, params object[] args)
        {
            StaticLogger.LogWarningFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorSlim(object message)
        {
            StaticLogger.LogErrorSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormatSlim(string format, params object[] args)
        {
            StaticLogger.LogErrorFormatSlim(format, args);
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Logging
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;
    using UnityEngine;

    partial class Logger
    {
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogInfo(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogWarning(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogError(message);
        }
    }
}

#endif
