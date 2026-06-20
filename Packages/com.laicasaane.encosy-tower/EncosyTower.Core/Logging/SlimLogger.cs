namespace EncosyTower.Logging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    /// <summary>
    /// A slim logger that omits stack traces for performance.
    /// </summary>
    /// <seealso cref="LogOption.NoStacktrace"/>
    public partial class SlimLogger : ILogger, IUnityLogger
    {
        public static readonly SlimLogger Default = new();

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            StaticLogger.LogException(value);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            StaticLogger.LogInfoSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            StaticLogger.LogInfoFormatSlim(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            StaticLogger.LogWarningSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            StaticLogger.LogWarningFormatSlim(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            StaticLogger.LogErrorSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            StaticLogger.LogErrorFormatSlim(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(UnityEngine.Object context, Exception value)
        {
            StaticLogger.LogErrorSlim(context, value);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(UnityEngine.Object context, object message)
        {
            StaticLogger.LogInfoSlim(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticLogger.LogInfoFormatSlim(context, format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(UnityEngine.Object context, object message)
        {
            StaticLogger.LogWarningSlim(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticLogger.LogWarningFormatSlim(context, format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(UnityEngine.Object context, object message)
        {
            StaticLogger.LogErrorSlim(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticLogger.LogErrorFormatSlim(context, format, args);
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

    partial class SlimLogger : IFixedLogger
    {
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogInfoSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogWarningSlim(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogErrorSlim(message);
        }
    }
}

#endif
