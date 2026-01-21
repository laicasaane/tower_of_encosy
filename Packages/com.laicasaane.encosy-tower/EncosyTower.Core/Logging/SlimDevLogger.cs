namespace EncosyTower.Logging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    /// <summary>
    /// A logger for Editor and Development environments that omits stack traces for performance.
    /// </summary>
    /// <remarks>
    /// In a Release build, its methods will do nothing.
    /// </remarks>
    /// <seealso cref="LogOption.NoStacktrace"/>
    public partial class SlimDevLogger : ILogger
    {
        public static readonly SlimDevLogger Default = new();

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            StaticDevLogger.LogException(value);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            StaticDevLogger.LogInfoSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            StaticDevLogger.LogInfoFormatSlim(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            StaticDevLogger.LogWarningSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            StaticDevLogger.LogWarningFormatSlim(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            StaticDevLogger.LogErrorSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
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

    partial class SlimDevLogger
    {
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticDevLogger.LogInfoSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticDevLogger.LogWarningSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticDevLogger.LogErrorSlim(message);
        }
    }
}

#endif
