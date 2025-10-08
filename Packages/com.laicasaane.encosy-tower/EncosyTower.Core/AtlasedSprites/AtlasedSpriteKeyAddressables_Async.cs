#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine;

namespace EncosyTower.AtlasedSprites
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask<Sprite>;
    using UnityTaskOpt = Cysharp.Threading.Tasks.UniTask<Option<Sprite>>;
#else
    using UnityTask = UnityEngine.Awaitable<UnityEngine.Sprite>;
    using UnityTaskOpt = UnityEngine.Awaitable<Option<UnityEngine.Sprite>>;
#endif

    partial struct AtlasedSpriteKeyAddressables : ILoadAsync<Sprite>, ITryLoadAsync<Sprite>
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
            if (_atlas.IsValid == false || _sprite.IsValid == false)
            {
                return Option.None;
            }

            var result = await Atlas.TryLoadAsync(token);
            return result.HasValue ? result.GetValueOrThrow().TryGetSprite(_sprite) : Option.None;
        }
    }
}

#endif
#endif
