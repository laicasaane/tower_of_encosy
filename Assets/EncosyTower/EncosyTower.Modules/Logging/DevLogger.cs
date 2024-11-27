using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Logging
{
    public class DevLogger : ILogger
    {
        public static readonly DevLogger Default = new();

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            DevLoggerAPI.LogException(value);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            DevLoggerAPI.LogInfo(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            DevLoggerAPI.LogInfoFormat(format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            DevLoggerAPI.LogWarning(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            DevLoggerAPI.LogWarningFormat(format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            DevLoggerAPI.LogError(message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            DevLoggerAPI.LogErrorFormat(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoSlim(object message)
        {
            DevLoggerAPI.LogInfoSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormatSlim(string format, params object[] args)
        {
            DevLoggerAPI.LogInfoFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningSlim(object message)
        {
            DevLoggerAPI.LogWarningSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormatSlim(string format, params object[] args)
        {
            DevLoggerAPI.LogWarningFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorSlim(object message)
        {
            DevLoggerAPI.LogErrorSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormatSlim(string format, params object[] args)
        {
            DevLoggerAPI.LogErrorFormatSlim(format, args);
        }
    }
}
