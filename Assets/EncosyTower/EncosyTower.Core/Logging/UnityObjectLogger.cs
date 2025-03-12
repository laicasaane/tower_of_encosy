using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Logging
{
    public readonly record struct UnityObjectLogger(UnityEngine.Object Context, LogEnvironment Environment)
    {
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogException(Context, value);
            else
                DevLoggerAPI.LogException(Context, value);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogInfo(Context, message);
            else
                DevLoggerAPI.LogInfo(Context, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogInfoFormat(Context, format, args);
            else
                DevLoggerAPI.LogInfoFormat(Context, format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogWarning(Context, message);
            else
                DevLoggerAPI.LogWarning(Context, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogWarningFormat(Context, format, args);
            else
                DevLoggerAPI.LogWarningFormat(Context, format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogError(Context, message);
            else
                DevLoggerAPI.LogError(Context, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogErrorFormat(Context, format, args);
            else
                DevLoggerAPI.LogErrorFormat(Context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoSlim(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogInfoSlim(Context, message);
            else
                DevLoggerAPI.LogInfoSlim(Context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormatSlim(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogInfoFormatSlim(Context, format, args);
            else
                DevLoggerAPI.LogInfoFormatSlim(Context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningSlim(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogWarningSlim(Context, message);
            else
                DevLoggerAPI.LogWarningSlim(Context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormatSlim(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogWarningFormatSlim(Context, format, args);
            else
                DevLoggerAPI.LogWarningFormatSlim(Context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorSlim(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogErrorSlim(Context, message);
            else
                DevLoggerAPI.LogErrorSlim(Context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormatSlim(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                RuntimeLoggerAPI.LogErrorFormatSlim(Context, format, args);
            else
                DevLoggerAPI.LogErrorFormatSlim(Context, format, args);
        }
    }

    public static class UnityObjectLoggerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityObjectLogger GetLogger(this UnityEngine.Object context, LogEnvironment environment)
            => new(context, environment);
    }
}
