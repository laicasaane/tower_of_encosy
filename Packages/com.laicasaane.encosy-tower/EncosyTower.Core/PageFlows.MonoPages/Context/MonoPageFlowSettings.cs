using EncosyTower.Logging;
using EncosyTower.Pooling;
using EncosyTower.Settings;
using Unity.Properties;
using UnityEngine.Serialization;

namespace EncosyTower.PageFlows.MonoPages
{
    [Settings(SettingsUsage.RuntimeProject, "Encosy Tower/Mono Page Flow")]
    [GeneratePropertyBag]
    public sealed partial class MonoPageFlowSettings : Settings<MonoPageFlowSettings>
    {
        public bool slimPublishingContext = true;
        public bool ignoreEmptySubscriber = true;
        public MonoPageLoaderStrategy loaderStrategy;

        public RentingStrategy poolRentingStrategy;

        [FormerlySerializedAs("pooledGameObjectStrategy")]
        public ReturningStrategy poolReturningStrategy;

        public MonoMessageScope messageScope;
        public LogEnvironment logEnvironment;
    }
}
