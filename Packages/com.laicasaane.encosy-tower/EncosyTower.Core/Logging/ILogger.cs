using System;

namespace EncosyTower.Logging
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
    }
}
