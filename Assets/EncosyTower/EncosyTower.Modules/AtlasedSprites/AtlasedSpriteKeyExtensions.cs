using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.Modules.AtlasedSprites
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
        public static Option<Sprite> TryGetSprite([NotNull] this SpriteAtlas atlas, string name)
        {
            if (atlas == false) return default;

            var sprite = atlas.GetSprite(name);
            return sprite ? (Option<Sprite>)sprite : default;
        }

        #region SERIALIZABLE
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey.Serializable FormatAtlas<T0>(this AtlasedSpriteKey.Serializable key, T0 arg0)
            => new(string.Format(key.Atlas, arg0), key.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey.Serializable FormatAtlas<T0, T1>(this AtlasedSpriteKey.Serializable key, T0 arg0, T1 arg1)
            => new(string.Format(key.Atlas, arg0, arg1), key.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey.Serializable FormatAtlas<T0, T1, T2>(this AtlasedSpriteKey.Serializable key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format(key.Atlas, arg0, arg1, arg2), key.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey.Serializable FormatSprite<T0>(this AtlasedSpriteKey.Serializable key, T0 arg0)
            => new(key.Atlas, string.Format(key.Sprite, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey.Serializable FormatSprite<T0, T1>(this AtlasedSpriteKey.Serializable key, T0 arg0, T1 arg1)
            => new(key.Atlas, string.Format(key.Sprite, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKey.Serializable FormatSprite<T0, T1, T2>(this AtlasedSpriteKey.Serializable key, T0 arg0, T1 arg1, T2 arg2)
            => new(key.Atlas, string.Format(key.Sprite, arg0, arg1, arg2));
    }
}
