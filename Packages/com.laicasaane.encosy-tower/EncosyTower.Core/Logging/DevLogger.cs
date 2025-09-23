namespace EncosyTower.Logging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    /// <summary>
    /// A logger for Editor and Development environments.
    /// </summary>
    /// <remarks>
    /// In a Release build, its methods will do nothing.
    /// </remarks>
    public partial class DevLogger : ILogger
    {
        public static readonly DevLogger Default = new();

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            StaticDevLogger.LogException(value);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            StaticDevLogger.LogInfo(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            StaticDevLogger.LogInfoFormat(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            StaticDevLogger.LogWarning(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            StaticDevLogger.LogWarningFormat(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            StaticDevLogger.LogError(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            StaticDevLogger.LogErrorFormat(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoSlim(object message)
        {
            StaticDevLogger.LogInfoSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormatSlim(string format, params object[] args)
        {
            StaticDevLogger.LogInfoFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningSlim(object message)
        {
            StaticDevLogger.LogWarningSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormatSlim(string format, params object[] args)
        {
            StaticDevLogger.LogWarningFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorSlim(object message)
        {
            StaticDevLogger.LogErrorSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormatSlim(string format, params object[] args)
        {
            StaticDevLogger.LogErrorFormatSlim(format, args);
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Logging
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Unity.Collections;
    using UnityEngine;

    partial class DevLogger
    {
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticDevLogger.LogInfo(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticDevLogger.LogWarning(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticDevLogger.LogError(message);
        }
    }
}

#endif
