using System;
using System.Diagnostics;
using EncosyTower.Annotations;
using EncosyTower.AtlasedSprites;
using EncosyTower.Logging;
using EncosyTower.ResourceKeys;
using EncosyTower.Variants;
using EncosyTower.Variants.Converters;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.AtlasedSprites
{
    [Serializable]
    [Label("Resources.Load(Atlased Sprite Key)", "Default")]
    [Adapter(sourceType: typeof(AtlasedSpriteKey), destType: typeof(Sprite), order: 0)]
    public sealed class ResourceAtlasedSpriteKeyAdapter : IAdapter
    {
        private readonly CachedVariantConverter<AtlasedSpriteKey> _keyConverter;
        private readonly CachedVariantConverter<Sprite> _assetConverter;

        public ResourceAtlasedSpriteKeyAdapter()
        {
            _keyConverter = CachedVariantConverter<AtlasedSpriteKey>.Default;
            _assetConverter = CachedVariantConverter<Sprite>.Default;
        }

        public Variant Convert(in Variant variant)
        {
            if (TryGetNames(variant, out var atlasName, out var spriteName))
            {
                var key = new ResourceKey<SpriteAtlas>(atlasName);
                var result = key.TryLoad();

                if (result.TryGetValue(out var atlas) && atlas.IsValid())
                {
                    var sprite = atlas.GetSprite(spriteName);

                    if (sprite)
                    {
                        return _assetConverter.ToVariantT(sprite);
                    }

                    ErrorFoundNoSprite(atlasName, spriteName, atlas);
                }
                else
                {
                    ErrorFoundNoAtlas(atlasName);
                }
            }

            return variant;
        }

        private bool TryGetNames(in Variant variant, out string atlas, out string sprite)
        {
            if (_keyConverter.TryGetValue(variant, out var key) && key.IsValid)
            {
                atlas = (string)key.Atlas;
                sprite = (string)key.Sprite;
                return true;
            }

            atlas = string.Empty;
            sprite = string.Empty;
            return false;
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoAtlas(string atlas)
        {
            StaticDevLogger.LogErrorFormat("Cannot find SpriteAtlas {0} in Resources."
                , atlas
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoSprite(string atlas, string sprite, UnityEngine.Object context)
        {
            StaticDevLogger.LogErrorFormat(context
                , "SpriteAtlas {0} does not contain sprite {1}"
                , atlas
                , sprite
            );
        }
    }
}
