using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.Modules.AtlasedSprites
{
    public readonly struct AtlasedSpriteKey : IEquatable<AtlasedSpriteKey>
    {
        public readonly AssetKey<SpriteAtlas> Atlas;
        public readonly AssetKey<Sprite> Sprite;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtlasedSpriteKey(AssetKey<SpriteAtlas> atlas, AssetKey<Sprite> sprite)
        {
            Atlas = atlas;
            Sprite = sprite;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Atlas.IsValid && Sprite.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AtlasedSpriteKey other)
            => Atlas.Equals(other.Atlas) && Sprite.Equals(other.Sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is AtlasedSpriteKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Atlas.GetHashCode(), Sprite.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"{Atlas},{Sprite}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AtlasedSpriteKey left, AtlasedSpriteKey right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AtlasedSpriteKey left, AtlasedSpriteKey right)
            => !left.Equals(right);

        [Serializable]
        public struct Serializable : ITryConvert<AtlasedSpriteKey>
            , IEquatable<Serializable>
        {
            [SerializeField]
            private string _atlas;

            [SerializeField]
            private string _sprite;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(string atlas, string sprite)
            {
                _atlas = atlas;
                _sprite = sprite;
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => string.IsNullOrEmpty(_atlas) == false && string.IsNullOrEmpty(_sprite) == false;
            }

            public string Atlas
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _atlas;
            }

            public string Sprite
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _sprite;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out AtlasedSpriteKey result)
            {
                result = new(_atlas, _sprite);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => string.Equals(_atlas, other._atlas, StringComparison.Ordinal)
                && string.Equals(_sprite, other._sprite, StringComparison.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable other && Equals(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => HashCode.Combine(_atlas, _sprite);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => $"{_atlas},{_sprite}";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator AtlasedSpriteKey(Serializable value)
                => new(value._atlas, value._sprite);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(AtlasedSpriteKey value)
                => new((string)value.Atlas, (string)value.Sprite);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left.Equals(right);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => !left.Equals(right);
        }
    }
}