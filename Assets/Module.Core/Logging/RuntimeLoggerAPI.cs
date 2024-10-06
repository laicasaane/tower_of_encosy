using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Module.Core.Logging
{
    using UnityDebug = UnityEngine.Debug;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// This logger works across environments, include Release build.
    /// </summary>
    public static partial class RuntimeLoggerAPI
    {
        [HideInCallstack, DoesNotReturn, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CallerInfo GetCallerInfo(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            return new CallerInfo(lineNumber, memberName, filePath);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogException(System.Exception value)
        {
            UnityDebug.LogException(value);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogInfo(object message)
        {
            UnityDebug.Log(message);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogInfoFormat(string format, params object[] args)
        {
            UnityDebug.LogFormat(format, args);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogWarning(object message)
        {
            UnityDebug.LogWarning(message);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogWarningFormat(string format, params object[] args)
        {
            UnityDebug.LogWarningFormat(format, args);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogWarningFormat(UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogWarningFormat(context, format, args);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogError(object message)
        {
            UnityDebug.LogError(message);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogErrorFormat(string format, params object[] args)
        {
            UnityDebug.LogErrorFormat(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogInfoSlim(object message)
        {
            DebugLogSlim(LogType.Log, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogInfoFormatSlim(string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Log, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogWarningSlim(object message)
        {
            DebugLogSlim(LogType.Warning, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogWarningFormatSlim(string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Warning, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogErrorSlim(object message)
        {
            DebugLogSlim(LogType.Error, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogErrorFormatSlim(string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Error, format, args);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogException(UnityObject context, System.Exception exception)
        {
            UnityDebug.LogException(exception, context);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogInfo(UnityObject context, object message)
        {
            UnityDebug.Log(message, context);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogInfoFormat(UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogFormat(context, format, args);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogWarning(UnityObject context, object message)
        {
            UnityDebug.LogWarning(message, context);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogError(UnityObject context, object message)
        {
            UnityDebug.LogError(message, context);
        }

        [HideInCallstack, DoesNotReturn]
        public static void LogErrorFormat(UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogErrorFormat(context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogInfoSlim(UnityObject context, object message)
        {
            DebugLogSlim(LogType.Log, context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogInfoFormatSlim(UnityObject context, string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Log, context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogWarningSlim(UnityObject context, object message)
        {
            DebugLogSlim(LogType.Warning, context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogWarningFormatSlim(UnityObject context, string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Warning, context, format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogErrorSlim(UnityObject context, object message)
        {
            DebugLogSlim(LogType.Error, context, message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack, DoesNotReturn]
        public static void LogErrorFormatSlim(UnityObject context, string format, params object[] args)
        {
            DebugLogFormatSlim(LogType.Error, context, format, args);
        }

        [HideInCallstack, DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogSlim(LogType type, object message)
        {
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, null, message.ToString());
        }

        [HideInCallstack, DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogSlim(LogType type, UnityObject context, object message)
        {
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, context, message.ToString());
        }

        [HideInCallstack, DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogFormatSlim(LogType type, string format, params object[] args)
        {
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, null, format, args);
        }

        [HideInCallstack, DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DebugLogFormatSlim(LogType type, UnityObject context, string format, params object[] args)
        {
            UnityDebug.LogFormat(type, LogOption.NoStacktrace, context, format, args);
        }
    }
}
