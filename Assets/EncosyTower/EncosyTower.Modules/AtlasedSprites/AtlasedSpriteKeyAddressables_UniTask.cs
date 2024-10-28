#if UNITY_ADDRESSABLES
#if UNITASK

using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EncosyTower.Modules.AtlasedSprites
{
    public readonly partial struct AtlasedSpriteKeyAddressables : ILoadAsync<Sprite>, ITryLoadAsync<Sprite>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<Sprite> LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.ValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<Option<Sprite>> TryLoadAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(Sprite)) return default;

            var result = await Atlas.TryLoadAsync(token);
            return result.HasValue ? result.Value().TryGetSprite(Sprite) : default;
        }
    }
}

#endif
#endif
