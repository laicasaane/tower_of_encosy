using EncosyTower.Logging;
using EncosyTower.Settings;

namespace EncosyTower.PageFlows.MonoPages
{
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/Mono Page Flow")]
    public sealed class MonoPageFlowSettings : Settings<MonoPageFlowSettings>
    {
        public bool slimPublishingContext = true;
        public bool ignoreEmptySubscriber = true;
        public MonoPageLoaderStrategy loaderStrategy;
        public MonoMessageScope messageScope;
        public LogEnvironment logEnvironment;
    }
}
