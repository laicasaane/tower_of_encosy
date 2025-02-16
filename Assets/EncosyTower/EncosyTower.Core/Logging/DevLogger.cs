using System;
using UnityEngine;

namespace EncosyTower.Logging
{
    public class DevLogger : ILogger
    {
        public static readonly DevLogger Default = new();

        [HideInCallstack]
        public void LogException(Exception value)
        {
            DevLoggerAPI.LogException(value);
        }

        [HideInCallstack]
        public void LogInfo(object message)
        {
            DevLoggerAPI.LogInfo(message);
        }

        [HideInCallstack]
        public void LogInfoFormat(string format, params object[] args)
        {
            DevLoggerAPI.LogInfoFormat(format, args);
        }

        [HideInCallstack]
        public void LogWarning(object message)
        {
            DevLoggerAPI.LogWarning(message);
        }

        [HideInCallstack]
        public void LogWarningFormat(string format, params object[] args)
        {
            DevLoggerAPI.LogWarningFormat(format, args);
        }

        [HideInCallstack]
        public void LogError(object message)
        {
            DevLoggerAPI.LogError(message);
        }

        [HideInCallstack]
        public void LogErrorFormat(string format, params object[] args)
        {
            DevLoggerAPI.LogErrorFormat(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack]
        public void LogInfoSlim(object message)
        {
            DevLoggerAPI.LogInfoSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack]
        public void LogInfoFormatSlim(string format, params object[] args)
        {
            DevLoggerAPI.LogInfoFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack]
        public void LogWarningSlim(object message)
        {
            DevLoggerAPI.LogWarningSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack]
        public void LogWarningFormatSlim(string format, params object[] args)
        {
            DevLoggerAPI.LogWarningFormatSlim(format, args);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack]
        public void LogErrorSlim(object message)
        {
            DevLoggerAPI.LogErrorSlim(message);
        }

        /// <see cref="LogOption.NoStacktrace"/>
        [HideInCallstack]
        public void LogErrorFormatSlim(string format, params object[] args)
        {
            DevLoggerAPI.LogErrorFormatSlim(format, args);
        }
    }
}
