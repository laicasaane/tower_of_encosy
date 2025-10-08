using System;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.AtlasedSprites
{
    [Serializable]
    public struct AtlasedSpriteKey : IEquatable<AtlasedSpriteKey>
    {
        [SerializeField] private AssetKey<SpriteAtlas> _atlas;
        [SerializeField] private AssetKey<Sprite> _sprite;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtlasedSpriteKey(AssetKey<SpriteAtlas> atlas, AssetKey<Sprite> sprite)
        {
            _atlas = atlas;
            _sprite = sprite;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _atlas.IsValid && _sprite.IsValid;
        }

        public readonly AssetKey<SpriteAtlas> Atlas
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _atlas;
        }

        public readonly AssetKey<Sprite> Sprite
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sprite;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AtlasedSpriteKey other)
            => Atlas.Equals(other._atlas) && Sprite.Equals(other._sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is AtlasedSpriteKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashCode.Combine(_atlas, _sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => $"{_atlas}[{_sprite}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AtlasedSpriteKey left, AtlasedSpriteKey right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AtlasedSpriteKey left, AtlasedSpriteKey right)
            => !left.Equals(right);
    }
}
