namespace EncosyTower.Logging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public partial class Logger : ILogger, IUnityLogger
    {
        public static readonly Logger Default = new();

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            StaticLogger.LogException(value);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            StaticLogger.LogInfo(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            StaticLogger.LogInfoFormat(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            StaticLogger.LogWarning(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            StaticLogger.LogWarningFormat(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            StaticLogger.LogError(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            StaticLogger.LogErrorFormat(format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(UnityEngine.Object context, Exception value)
        {
            StaticLogger.LogException(context, value);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(UnityEngine.Object context, object message)
        {
            StaticLogger.LogInfo(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticLogger.LogInfoFormat(context, format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(UnityEngine.Object context, object message)
        {
            StaticLogger.LogWarning(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticLogger.LogWarningFormat(context, format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(UnityEngine.Object context, object message)
        {
            StaticLogger.LogError(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticLogger.LogErrorFormat(context, format, args);
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

    partial class Logger : IFixedLogger
    {
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogInfo(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogWarning(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            StaticLogger.LogError(message);
        }
    }
}

#endif
