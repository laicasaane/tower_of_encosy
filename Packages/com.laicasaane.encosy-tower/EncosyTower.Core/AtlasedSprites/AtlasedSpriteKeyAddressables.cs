#if UNITY_ADDRESSABLES

using System;
using System.Runtime.CompilerServices;
using EncosyTower.AddressableKeys;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.AtlasedSprites
{
    using Error = AtlasedSpriteKeyError;
    using ValueHandlePair = ValueHandlePair<Sprite, SpriteAtlas>;

    [Serializable]
    public partial struct AtlasedSpriteKeyAddressables : ILoad<Sprite>, ITryLoad<Sprite>, ILoadOrError<Sprite, Error>
        , IEquatable<AtlasedSpriteKeyAddressables>
    {
        [SerializeField] private AssetKey<SpriteAtlas> _atlas;
        [SerializeField] private AssetKey<Sprite> _sprite;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtlasedSpriteKeyAddressables(AtlasedSpriteKey value)
        {
            _atlas = value.Atlas;
            _sprite = value.Sprite;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _atlas.IsValid && _sprite.IsValid;
        }

        public readonly AddressableKey<SpriteAtlas> Atlas
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
        public static implicit operator AtlasedSpriteKeyAddressables(AtlasedSpriteKey value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtlasedSpriteKey(AtlasedSpriteKeyAddressables value)
            => new(value._atlas.Value, value._sprite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AtlasedSpriteKeyAddressables other)
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
        public static bool operator ==(AtlasedSpriteKeyAddressables left, AtlasedSpriteKeyAddressables right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AtlasedSpriteKeyAddressables left, AtlasedSpriteKeyAddressables right)
            => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Sprite Load()
            => TryLoad().GetValueOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ValueHandlePair LoadGetHandle()
            => TryLoadGetHandle().GetValueOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<Sprite> TryLoad()
        {
            var result = TryLoadGetHandle();
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Result<Sprite, Error> LoadOrError()
        {
            var result = LoadGetHandleOrError();

            if (result.TryGetValue(out var value))
            {
                return value.Value;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<ValueHandlePair> TryLoadGetHandle()
        {
            var result = LoadGetHandleOrError();
            return result.Value;
        }

        public readonly Result<ValueHandlePair, Error> LoadGetHandleOrError()
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AtlasedSpriteKey)this);
            }

            var atlasResult = Atlas.LoadGetHandleOrError();

            if (atlasResult.TryGetError(out var atlasError))
            {
                return Error.From(atlasError, (AtlasedSpriteKey)this);
            }

            if (atlasResult.TryGetValue(out var atlasValue) == false)
            {
                return Error.Undefined((AtlasedSpriteKey)this);
            }

            var spriteResult = atlasValue.Value.GetSpriteOrError(_sprite);

            if (spriteResult.TryGetValue(out var spriteValue))
            {
                return new ValueHandlePair(spriteValue, atlasValue.Handle);
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
        public static AtlasedSpriteKeyAddressables AsAddressable(this AtlasedSpriteKey value)
            => value;
    }
}

#endif
