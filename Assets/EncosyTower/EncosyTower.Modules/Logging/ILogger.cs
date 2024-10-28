using System;

namespace EncosyTower.Modules.Logging
{
    public interface ILogger
    {
        void LogException(Exception value);

        void LogInfo(object message);

        void LogInfoFormat(string format, params object[] args);

        void LogWarning(object message);

        void LogWarningFormat(string format, params object[] args);

        void LogError(object message);

        void LogErrorFormat(string format, params object[] args);

        void LogInfoSlim(object message);

        void LogInfoFormatSlim(string format, params object[] args);

        void LogWarningSlim(object message);

        void LogWarningFormatSlim(string format, params object[] args);

        void LogErrorSlim(object message);

        void LogErrorFormatSlim(string format, params object[] args);
    }
}
