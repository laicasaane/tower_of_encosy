using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.Modules.AtlasedSprites
{
    public readonly partial struct AtlasedSpriteKeyResources : ILoad<Sprite>, ITryLoad<Sprite>
    {
        public readonly ResourceKey<SpriteAtlas> Atlas;
        public readonly string Sprite;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtlasedSpriteKeyResources(AtlasedSpriteKey value)
        {
            Atlas = value.Atlas;
            Sprite = (string)value.Sprite;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKeyResources(AtlasedSpriteKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKey(AtlasedSpriteKeyResources value)
            => new(value.Atlas.Value, value.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKeyResources(AtlasedSpriteKey.Serializable value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKey.Serializable(AtlasedSpriteKeyResources value)
            => new((string)value.Atlas.Value, value.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sprite Load()
            => TryLoad().ValueOrDefault();

        public Option<Sprite> TryLoad()
        {
            if (string.IsNullOrEmpty(Sprite)) return default;

            var atlasOpt = Atlas.TryLoad();
            return atlasOpt.HasValue ? atlasOpt.Value().TryGetSprite(Sprite) : default;
        }
    }

    public static partial class AtlasedSpriteKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKeyResources ViaResources(this AtlasedSpriteKey value)
            => value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKeyResources ViaResources(this AtlasedSpriteKey.Serializable value)
            => value;
    }
}

