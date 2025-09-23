namespace EncosyTower.Logging
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using EncosyTower.Common;
    using UnityEngine;

    using UnityDebug = UnityEngine.Debug;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// A static logger for Editor, Development and Release environments.
    /// </summary>
    public static partial class StaticLogger
    {
#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogException(System.Exception value)
        {
            UnityDebug.LogException(value);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogInfo(object message)
        {
            UnityDebug.Log(message);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogInfoFormat(string format, params object[] args)
        {
            UnityDebug.LogFormat(format, args);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogWarning(object message)
        {
            UnityDebug.LogWarning(message);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogWarningFormat(string format, params object[] args)
        {
            UnityDebug.LogWarningFormat(format, args);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogError(object message)
        {
            UnityDebug.LogError(message);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogErrorFormat(string format, params object[] args)
        {
            UnityDebug.LogErrorFormat(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogInfoSlim(object message)
        {
            DebugLogSlim(LogType.Log, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogInfoFormatSlim(string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Log, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogWarningSlim(object message)
        {
            DebugLogSlim(LogType.Warning, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogWarningFormatSlim(string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Warning, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogErrorSlim(object message)
        {
            DebugLogSlim(LogType.Error, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogErrorFormatSlim(string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Error, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogException(UnityObject context, System.Exception exception)
        {
            UnityDebug.LogException(exception, context);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogInfo(UnityObject context, object message)
        {
            UnityDebug.Log(message, context);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogInfoFormat(UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogFormat(context, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogWarning(UnityObject context, object message)
        {
            UnityDebug.LogWarning(message, context);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogWarningFormat(UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogWarningFormat(context, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogError(UnityObject context, object message)
        {
            UnityDebug.LogError(message, context);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden]
        public static void LogErrorFormat(UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogErrorFormat(context, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogInfoSlim(UnityObject context, object message)
        {
            DebugLogSlim(LogType.Log, context, message);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogInfoFormatSlim(UnityObject context, string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Log, context, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogWarningSlim(UnityObject context, object message)
        {
            DebugLogSlim(LogType.Warning, context, message);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogWarningFormatSlim(UnityObject context, string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Warning, context, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogErrorSlim(UnityObject context, object message)
        {
            DebugLogSlim(LogType.Error, context, message);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, StackTraceHidden]
        public static void LogErrorFormatSlim(UnityObject context, string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Error, context, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogSlim(LogType type, object message)
        {
            var msg = message.GetString(CultureInfo.CurrentCulture);
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, null, "{0}", msg);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogSlim(LogType type, UnityObject context, object message)
        {
            var msg = message.GetString(CultureInfo.CurrentCulture);
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, context, "{0}", msg);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogFormatSlim(LogType type, string format, params object[] args)
        {
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, null, format, args);
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogFormatSlim(LogType type, UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, context, format, args);
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Logging
{
    using System.Diagnostics;
    using Unity.Collections;
    using UnityEngine;

    using UnityDebug = UnityEngine.Debug;

    partial class StaticLogger
    {
        [HideInCallstack, StackTraceHidden]
        public static void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            UnityDebug.Log(message);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            UnityDebug.LogWarning(message);
        }

        [HideInCallstack, StackTraceHidden]
        public static void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            UnityDebug.LogError(message);
        }
    }
}

#endif
