#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.AddressableKeys;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine;
using UnityEngine.U2D;

namespace EncosyTower.AtlasedSprites
{
    using Error = AtlasedSpriteKeyError;
    using ValueHandlePair = ValueHandlePair<Sprite, SpriteAtlas>;

#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask<Sprite>;
    using UnityTaskHandle = Cysharp.Threading.Tasks.UniTask<ValueHandlePair<Sprite, SpriteAtlas>>;
    using UnityTaskOpt = Cysharp.Threading.Tasks.UniTask<Option<Sprite>>;
    using UnityTaskResult = Cysharp.Threading.Tasks.UniTask<Result<Sprite, AtlasedSpriteKeyError>>;
    using UnityTaskHandleOpt = Cysharp.Threading.Tasks.UniTask<Option<ValueHandlePair<Sprite, SpriteAtlas>>>;
    using UnityTaskHandleResult = Cysharp.Threading.Tasks.UniTask<Result<ValueHandlePair<Sprite, SpriteAtlas>, AtlasedSpriteKeyError>>;
#else
    using UnityTask = UnityEngine.Awaitable<UnityEngine.Sprite>;
    using UnityTaskHandle = UnityEngine.Awaitable<ValueHandlePair<UnityEngine.Sprite, SpriteAtlas>>;
    using UnityTaskOpt = UnityEngine.Awaitable<Option<UnityEngine.Sprite>>;
    using UnityTaskResult = UnityEngine.Awaitable<Result<UnityEngine.Sprite, AtlasedSpriteKeyError>>;
    using UnityTaskHandleOpt = UnityEngine.Awaitable<Option<ValueHandlePair<UnityEngine.Sprite, SpriteAtlas>>>;
    using UnityTaskHandleResult = UnityEngine.Awaitable<Result<ValueHandlePair<UnityEngine.Sprite, SpriteAtlas>, AtlasedSpriteKeyError>>;
#endif

    partial struct AtlasedSpriteKeyAddressables : ILoadAsync<Sprite>, ITryLoadAsync<Sprite>, ILoadOrErrorAsync<Sprite, Error>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTaskHandle LoadGetHandleAsync(CancellationToken token = default)
        {
            var result = await TryLoadGetHandleAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTaskOpt TryLoadAsync(CancellationToken token = default)
        {
            var result = await LoadOrErrorAsync(token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTaskHandleOpt TryLoadGetHandleAsync(CancellationToken token = default)
        {
            var result = await LoadGetHandleOrErrorAsync(token);
            return result.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTaskResult LoadOrErrorAsync(CancellationToken token = default)
        {
            var result = await LoadGetHandleOrErrorAsync(token);

            if (result.TryGetValue(out var value))
            {
                return value.Value;
            }

            if (result.TryGetError(out var error))
            {
                return error;
            }

            return Error.Undefined((AtlasedSpriteKey)this);
        }

        public readonly async UnityTaskHandleResult LoadGetHandleOrErrorAsync(CancellationToken token = default)
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AtlasedSpriteKey)this);
            }

            var atlasResult = await Atlas.LoadGetHandleOrErrorAsync(token);

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
}

#endif
#endif
