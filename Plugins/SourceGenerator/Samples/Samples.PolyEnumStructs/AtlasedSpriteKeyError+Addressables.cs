#if UNITY_ADDRESSABLES

using EncosyTower.AddressableKeys;
using EncosyTower.AtlasedSprites;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Samples.PolyEnumStructs.Errors3
{
    partial struct AtlasedSpriteKeyError
    {
        public static AtlasedSpriteKeyError From(AddressableKeyError error)
        {
            return default;
        }

        partial struct Error
        {
            public partial record struct FailedStatus(AtlasedSpriteKey Key, AsyncOperationStatus Status)
            {
                public string ToMessage(string prefix)
                {
                    return default;
                }

                public void Log(EncosyTower.Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }
        }
    }
}

#endif
