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
                StaticLogger.LogException(Context, value);
            else
                StaticDevLogger.LogException(Context, value);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogInfo(Context, message);
            else
                StaticDevLogger.LogInfo(Context, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogInfoFormat(Context, format, args);
            else
                StaticDevLogger.LogInfoFormat(Context, format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogWarning(Context, message);
            else
                StaticDevLogger.LogWarning(Context, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogWarningFormat(Context, format, args);
            else
                StaticDevLogger.LogWarningFormat(Context, format, args);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogError(Context, message);
            else
                StaticDevLogger.LogError(Context, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogErrorFormat(Context, format, args);
            else
                StaticDevLogger.LogErrorFormat(Context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoSlim(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogInfoSlim(Context, message);
            else
                StaticDevLogger.LogInfoSlim(Context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormatSlim(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogInfoFormatSlim(Context, format, args);
            else
                StaticDevLogger.LogInfoFormatSlim(Context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningSlim(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogWarningSlim(Context, message);
            else
                StaticDevLogger.LogWarningSlim(Context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormatSlim(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogWarningFormatSlim(Context, format, args);
            else
                StaticDevLogger.LogWarningFormatSlim(Context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorSlim(object message)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogErrorSlim(Context, message);
            else
                StaticDevLogger.LogErrorSlim(Context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormatSlim(string format, params object[] args)
        {
            if (Environment == LogEnvironment.Runtime)
                StaticLogger.LogErrorFormatSlim(Context, format, args);
            else
                StaticDevLogger.LogErrorFormatSlim(Context, format, args);
        }
    }

    public static class UnityObjectLoggerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityObjectLogger GetLogger(this UnityEngine.Object context, LogEnvironment environment)
            => new(context, environment);
    }
}
