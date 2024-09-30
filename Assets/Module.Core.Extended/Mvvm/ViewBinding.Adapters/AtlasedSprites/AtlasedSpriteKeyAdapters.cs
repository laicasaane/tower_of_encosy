#if UNITY_ADDRESSABLES

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Module.Core.AtlasedSprites;
using Module.Core.Logging;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;
using Module.Core.Unions.Converters;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace Module.Core.Extended.Mvvm.ViewBinding.Adapters.AtlasedSprites
{
    public abstract class AddressableAtlasedSpriteAdapter : IAdapter
    {
        private readonly CachedUnionConverter<Sprite> _converter = new();

        protected abstract bool TryGetNames(in Union union, out string atlas, out string sprite);

        public Union Convert(in Union union)
        {
            if (TryGetNames(union, out var atlasName, out var spriteName))
            {
                var handle = Addressables.LoadAssetAsync<SpriteAtlas>(atlasName);
                var atlas = handle.WaitForCompletion();

                if (atlas)
                {
                    var sprite = atlas.GetSprite(spriteName);

                    if (sprite)
                    {
                        return _converter.ToUnionT(sprite);
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

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoAtlas(string atlas)
        {
            DevLoggerAPI.LogErrorFormat("Cannot find SpriteAtlas {0} in Addressables."
                , atlas
            );
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoSprite(string atlas, string sprite, UnityEngine.Object context)
        {
            DevLoggerAPI.LogErrorFormat(context
                , "SpriteAtlas {0} does not contain sprite {1}"
                , atlas
                , sprite
            );
        }
    }

    [Serializable]
    [Label("Addressables.Load(Atlased Sprite Key)", "Default")]
    [Adapter(sourceType: typeof(AtlasedSpriteKey), destType: typeof(Sprite), order: 0)]
    public sealed class AddressableAtlasedSpriteKeyAdapter : AddressableAtlasedSpriteAdapter
    {
        protected override bool TryGetNames(in Union union, out string atlas, out string sprite)
        {
            var converter = Union<AtlasedSpriteKey>.GetConverter();

            if (converter.TryGetValue(union, out var key) && key.IsValid)
            {
                atlas = key.Atlas;
                sprite = key.Sprite;
                return true;
            }

            atlas = string.Empty;
            sprite = string.Empty;
            return false;
        }
    }

    [Serializable]
    [Label("Addressables.Load(Atlased Sprite Key Serializable)", "Default")]
    [Adapter(sourceType: typeof(AtlasedSpriteKey.Serializable), destType: typeof(Sprite), order: 0)]
    public sealed class AddressableAtlasedSpriteKeySerializableAdapter : AddressableAtlasedSpriteAdapter
    {
        protected override bool TryGetNames(in Union union, out string atlas, out string sprite)
        {
            var converter = Union<AtlasedSpriteKey.Serializable>.GetConverter();

            if (converter.TryGetValue(union, out var key) && key.IsValid)
            {
                atlas = key.Atlas;
                sprite = key.Sprite;
                return true;
            }

            atlas = string.Empty;
            sprite = string.Empty;
            return false;
        }
    }
}

#endif
