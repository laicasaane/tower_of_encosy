using System;

namespace EncosyTower.Logging
{
    using UnityObject = UnityEngine.Object;

    public partial interface IUnityLogger : ILogger
    {
        void LogException(UnityObject context, Exception value);

        void LogInfo(UnityObject context, object message);

        void LogInfoFormat(UnityObject context, string format, params object[] args);

        void LogWarning(UnityObject context, object message);

        void LogWarningFormat(UnityObject context, string format, params object[] args);

        void LogError(UnityObject context, object message);

        void LogErrorFormat(UnityObject context, string format, params object[] args);
    }
}
