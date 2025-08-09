#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Loaders;
using UnityEngine;

namespace EncosyTower.AtlasedSprites
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask<Sprite>;
    using UnityTaskOpt = Cysharp.Threading.Tasks.UniTask<Common.Option<Sprite>>;
#else
    using UnityTask = UnityEngine.Awaitable<UnityEngine.Sprite>;
    using UnityTaskOpt = UnityEngine.Awaitable<Common.Option<UnityEngine.Sprite>>;
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
            if (string.IsNullOrEmpty(Sprite)) return default;

            var result = await Atlas.TryLoadAsync(token);
            return result.HasValue ? result.GetValueOrThrow().TryGetSprite(Sprite) : default;
        }
    }
}

#endif
#endif
