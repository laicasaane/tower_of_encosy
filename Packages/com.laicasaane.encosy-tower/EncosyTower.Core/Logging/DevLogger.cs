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
    public partial class DevLogger : ILogger, IUnityLogger
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

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(UnityEngine.Object context, Exception value)
        {
            StaticDevLogger.LogException(context, value);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(UnityEngine.Object context, object message)
        {
            StaticDevLogger.LogInfo(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticDevLogger.LogInfoFormat(context, format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(UnityEngine.Object context, object message)
        {
            StaticDevLogger.LogWarning(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticDevLogger.LogWarningFormat(context, format, args);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(UnityEngine.Object context, object message)
        {
            StaticDevLogger.LogError(context, message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            StaticDevLogger.LogErrorFormat(context, format, args);
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

    partial class DevLogger : IFixedLogger
    {
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            UnityEngine.Debug.Log(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}

#endif
