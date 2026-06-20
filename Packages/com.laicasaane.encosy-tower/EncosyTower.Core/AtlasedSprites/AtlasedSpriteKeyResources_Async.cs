#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine;

namespace EncosyTower.AtlasedSprites
{
    using Error = AtlasedSpriteKeyError;

#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask<Sprite>;
    using UnityTaskOpt = Cysharp.Threading.Tasks.UniTask<Option<Sprite>>;
    using UnityTaskResult = Cysharp.Threading.Tasks.UniTask<Result<Sprite, AtlasedSpriteKeyError>>;
#else
    using UnityTask = UnityEngine.Awaitable<UnityEngine.Sprite>;
    using UnityTaskOpt = UnityEngine.Awaitable<Option<UnityEngine.Sprite>>;
    using UnityTaskResult = UnityEngine.Awaitable<Result<UnityEngine.Sprite, AtlasedSpriteKeyError>>;
#endif

    partial struct AtlasedSpriteKeyResources : ILoadAsync<Sprite>, ITryLoadAsync<Sprite>, ILoadOrErrorAsync<Sprite, Error>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTask LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async UnityTaskOpt TryLoadAsync(CancellationToken token = default)
        {
            var result = await LoadOrErrorAsync(token);
            return result.Value;
        }

        public readonly async UnityTaskResult LoadOrErrorAsync(CancellationToken token = default)
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((AtlasedSpriteKey)this);
            }

            var atlasResult = await Atlas.LoadOrErrorAsync(token);

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
}

#endif
