#if UNITY_ADDRESSABLES

using System;
using System.Diagnostics;
using EncosyTower.AddressableKeys;
using EncosyTower.Annotations;
using EncosyTower.AtlasedSprites;
using EncosyTower.Logging;
using EncosyTower.Unions;
using EncosyTower.Unions.Converters;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.AtlasedSprites
{
    [Serializable]
    [Label("Addressables.Load(Atlased Sprite Key)", "Default")]
    [Adapter(sourceType: typeof(AtlasedSpriteKey), destType: typeof(Sprite), order: 0)]
    public sealed class AddressableAtlasedSpriteAdapter : IAdapter
    {
        private readonly CachedUnionConverter<AtlasedSpriteKey> _keyConverter;
        private readonly CachedUnionConverter<AtlasedSpriteKey.Serializable> _keySerializableConverter;
        private readonly CachedUnionConverter<Sprite> _assetConverter;

        public AddressableAtlasedSpriteAdapter()
        {
            _keyConverter = CachedUnionConverter<AtlasedSpriteKey>.Default;
            _keySerializableConverter = CachedUnionConverter<AtlasedSpriteKey.Serializable>.Default;
            _assetConverter = CachedUnionConverter<Sprite>.Default;
        }

        public Union Convert(in Union union)
        {
            if (TryGetNames(union, out var atlasName, out var spriteName))
            {
                var key = new AddressableKey<SpriteAtlas>(atlasName);
                var result = key.TryLoad();

                if (result.TryGetValue(out var atlas) && atlas.IsValid())
                {
                    var sprite = atlas.GetSprite(spriteName);

                    if (sprite)
                    {
                        return _assetConverter.ToUnionT(sprite);
                    }

                    ErrorFoundNoSprite(atlasName, spriteName, atlas);
                }
                else
                {
                    ErrorFoundNoAtlas(atlasName);
                }
            }

            return union;
        }

        private bool TryGetNames(in Union union, out string atlas, out string sprite)
        {
            if (_keyConverter.TryGetValue(union, out var key) && key.IsValid)
            {
                atlas = (string)key.Atlas;
                sprite = (string)key.Sprite;
                return true;
            }

            if (_keySerializableConverter.TryGetValue(union, out var keySerializable) && key.IsValid)
            {
                atlas = (string)keySerializable.Atlas;
                sprite = (string)keySerializable.Sprite;
                return true;
            }

            atlas = string.Empty;
            sprite = string.Empty;
            return false;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoAtlas(string atlas)
        {
            DevLoggerAPI.LogErrorFormat("Cannot find SpriteAtlas {0} in Addressables."
                , atlas
            );
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoSprite(string atlas, string sprite, UnityEngine.Object context)
        {
            DevLoggerAPI.LogErrorFormat(context
                , "SpriteAtlas {0} does not contain sprite {1}"
                , atlas
                , sprite
            );
        }
    }
}

#endif
