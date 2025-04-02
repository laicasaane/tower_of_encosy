using EncosyTower.Logging;
using EncosyTower.Settings;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/Mono Page Flow")]
    public sealed class MonoPageFlowSettings : Settings<MonoPageFlowSettings>
    {
        public MonoPageLoaderStrategy loaderStrategy;
        public MonoMessageScope messageScope;
        public LogEnvironment logEnvironment;
    }
}
