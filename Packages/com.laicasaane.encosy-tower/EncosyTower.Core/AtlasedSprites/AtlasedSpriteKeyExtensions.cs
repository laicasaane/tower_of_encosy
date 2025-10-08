using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.AtlasedSprites
{
    public static partial class AtlasedSpriteKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey FormatAtlas<T0>(this AtlasedSpriteKey key, T0 arg0)
            => new(string.Format((string)key.Atlas, arg0), key.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey FormatAtlas<T0, T1>(this AtlasedSpriteKey key, T0 arg0, T1 arg1)
            => new(string.Format((string)key.Atlas, arg0, arg1), key.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey FormatAtlas<T0, T1, T2>(this AtlasedSpriteKey key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format((string)key.Atlas, arg0, arg1, arg2), key.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey FormatSprite<T0>(this AtlasedSpriteKey key, T0 arg0)
            => new(key.Atlas, string.Format((string)key.Sprite, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey FormatSprite<T0, T1>(this AtlasedSpriteKey key, T0 arg0, T1 arg1)
            => new(key.Atlas, string.Format((string)key.Sprite, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey FormatSprite<T0, T1, T2>(this AtlasedSpriteKey key, T0 arg0, T1 arg1, T2 arg2)
            => new(key.Atlas, string.Format((string)key.Sprite, arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<Sprite> TryGetSprite([NotNull] this SpriteAtlas atlas, AssetKey<Sprite> name)
        {
            if (atlas == false || name.IsValid == false) return Option.None;

            var sprite = atlas.GetSprite(name.Value);
            return sprite ? (Option<Sprite>)sprite : Option.None;
        }
    }
}
