using EncosyTower.Logging;
using EncosyTower.Pooling;
using EncosyTower.Settings;
using Unity.Properties;

namespace EncosyTower.PageFlows.MonoPages
{
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/Mono Page Flow")]
    [GeneratePropertyBag]
    public sealed partial class MonoPageFlowSettings : Settings<MonoPageFlowSettings>
    {
        public bool warnNoSubscriber = false;
        public MonoPageLoaderStrategy loaderStrategy;

        public RentingStrategy poolRentingStrategy;
        public ReturningStrategy poolReturningStrategy;

        public MonoMessageScope messageScope;
        public LogEnvironment logEnvironment;
    }
}
