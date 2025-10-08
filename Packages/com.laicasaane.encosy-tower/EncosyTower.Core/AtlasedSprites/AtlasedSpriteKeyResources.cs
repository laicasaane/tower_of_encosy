using System;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.ResourceKeys;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.AtlasedSprites
{
    [Serializable]
    public partial struct AtlasedSpriteKeyResources : ILoad<Sprite>, ITryLoad<Sprite>
        , IEquatable<AtlasedSpriteKeyResources>
    {
        [SerializeField] private AssetKey<SpriteAtlas> _atlas;
        [SerializeField] private AssetKey<Sprite> _sprite;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtlasedSpriteKeyResources(AtlasedSpriteKey value)
        {
            _atlas = value.Atlas;
            _sprite = value.Sprite;
        }

        public readonly ResourceKey<SpriteAtlas> Atlas
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
        public static implicit operator AtlasedSpriteKeyResources(AtlasedSpriteKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKey(AtlasedSpriteKeyResources value)
            => new(value._atlas.Value, value._sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AtlasedSpriteKeyResources other)
            => Atlas.Equals(other._atlas) && Sprite.Equals(other._sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is AtlasedSpriteKeyResources other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashCode.Combine(_atlas, _sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => $"{_atlas}[{_sprite}]";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AtlasedSpriteKeyResources left, AtlasedSpriteKeyResources right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AtlasedSpriteKeyResources left, AtlasedSpriteKeyResources right)
            => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Sprite Load()
            => TryLoad().GetValueOrDefault();

        public readonly Option<Sprite> TryLoad()
        {
            if (_atlas.IsValid == false || _sprite.IsValid == false)
            {
                return Option.None;
            }

            var atlasOpt = Atlas.TryLoad();
            return atlasOpt.HasValue ? atlasOpt.GetValueOrThrow().TryGetSprite(Sprite) : Option.None;
        }
    }

    public static partial class AtlasedSpriteKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKeyResources ViaResources(this AtlasedSpriteKey value)
            => value;
    }
}

