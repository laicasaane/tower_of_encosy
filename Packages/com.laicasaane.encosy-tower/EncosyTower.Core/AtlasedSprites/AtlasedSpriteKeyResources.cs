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
    using Error = AtlasedSpriteKeyError;

    [Serializable]
    public partial struct AtlasedSpriteKeyResources : ILoad<Sprite>, ITryLoad<Sprite>, ILoadOrError<Sprite, Error>
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

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _atlas.IsValid && _sprite.IsValid;
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
            => HashValue.Combine(_atlas, _sprite);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<Sprite> TryLoad()
        {
            var result = LoadOrError();
            return result.Value;
        }

        public readonly Result<Sprite, Error> LoadOrError()
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AtlasedSpriteKey)this);
            }

            var atlasResult = Atlas.LoadOrError();

            if (atlasResult.TryGetError(out var atlasError))
            {
                return Error.From(atlasError, (AtlasedSpriteKey)this);
            }

            if (atlasResult.TryGetValue(out var atlasValue) == false)
            {
                return Error.Undefined((AtlasedSpriteKey)this);
            }

            var spriteResult = atlasValue.GetSpriteOrError(_sprite);

            if (spriteResult.TryGetValue(out var spriteValue))
            {
                return spriteValue;
            }

            if (spriteResult.TryGetError(out var spriteError))
            {
                return spriteError;
            }

            return Error.Undefined((AtlasedSpriteKey)this);
        }
    }

    public static partial class AtlasedSpriteKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AtlasedSpriteKeyResources AsResource(this AtlasedSpriteKey value)
            => value;
    }
}

