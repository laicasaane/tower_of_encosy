using EncosyTower.Logging;
using EncosyTower.Settings;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/Mono Page Flow")]
    public sealed class MonoPageFlowSettings : Settings<MonoPageFlowSettings>
    {
        [SerializeField] internal MonoPageLoaderStrategy _loaderStrategy;
        [SerializeField] internal MonoMessageScope _messageScope;
        [SerializeField] internal LogEnvironment _logEnvironment;

        public MonoPageLoaderStrategy LoaderStrategy
        {
            get => _loaderStrategy;
            set => _loaderStrategy = value;
        }

        public MonoMessageScope MessageScope
        {
            get => _messageScope;
            set => _messageScope = value;
        }

        public LogEnvironment LogEnvironment
        {
            get => _logEnvironment;
            set => _logEnvironment = value;
        }
    }
}
