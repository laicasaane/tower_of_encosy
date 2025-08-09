#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using EncosyTower.AddressableKeys;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.AtlasedSprites
{
    public readonly partial struct AtlasedSpriteKeyAddressables : ILoad<Sprite>, ITryLoad<Sprite>
    {
        public readonly AddressableKey<SpriteAtlas> Atlas;
        public readonly string Sprite;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtlasedSpriteKeyAddressables(AtlasedSpriteKey value)
        {
            Atlas = value.Atlas;
            Sprite = (string)value.Sprite;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKeyAddressables(AtlasedSpriteKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKey(AtlasedSpriteKeyAddressables value)
            => new(value.Atlas.Value, value.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKeyAddressables(AtlasedSpriteKey.Serializable value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKey.Serializable(AtlasedSpriteKeyAddressables value)
            => new((string)value.Atlas.Value, value.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sprite Load()
            => TryLoad().GetValueOrDefault();

        public Option<Sprite> TryLoad()
        {
            if (string.IsNullOrEmpty(Sprite)) return default;

            var atlasOpt = Atlas.TryLoad();
            return atlasOpt.HasValue ? atlasOpt.GetValueOrThrow().TryGetSprite(Sprite) : default;
        }
    }

    public static partial class AtlasedSpriteKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKeyAddressables ViaAddressables(this AtlasedSpriteKey value)
            => value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKeyAddressables ViaAddressables(this AtlasedSpriteKey.Serializable value)
            => value;
    }
}

#endif
