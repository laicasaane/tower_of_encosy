using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Module.Core.Logging
{
    public class RuntimeLogger : ILogger
    {
        public static readonly RuntimeLogger Default = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            RuntimeLoggerAPI.LogException(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            RuntimeLoggerAPI.LogInfo(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            RuntimeLoggerAPI.LogInfoFormat(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            RuntimeLoggerAPI.LogWarning(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            RuntimeLoggerAPI.LogWarningFormat(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            RuntimeLoggerAPI.LogError(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            RuntimeLoggerAPI.LogErrorFormat(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoSlim(object message)
        {
            RuntimeLoggerAPI.LogInfoSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormatSlim(string format, params object[] args)
        {
            RuntimeLoggerAPI.LogInfoFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningSlim(object message)
        {
            RuntimeLoggerAPI.LogWarningSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormatSlim(string format, params object[] args)
        {
            RuntimeLoggerAPI.LogWarningFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorSlim(object message)
        {
            RuntimeLoggerAPI.LogErrorSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormatSlim(string format, params object[] args)
        {
            RuntimeLoggerAPI.LogErrorFormatSlim(format, args);
        }
    }
}
