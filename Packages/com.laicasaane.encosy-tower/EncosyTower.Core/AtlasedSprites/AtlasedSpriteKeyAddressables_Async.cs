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

    public readonly partial struct AtlasedSpriteKeyAddressables : ILoadAsync<Sprite>, ITryLoadAsync<Sprite>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTaskOpt TryLoadAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(Sprite)) return Option.None;

            var result = await Atlas.TryLoadAsync(token);
            return result.HasValue ? result.GetValueOrThrow().TryGetSprite(Sprite) : Option.None;
        }
    }
}

#endif
#endif
