#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules
{
    partial record struct ResourceKey<T> : ILoadAsync<T>, ITryLoadAsync<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async Awaitable<T> LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.ValueOrDefault();
        }

        public readonly async Awaitable<Option<T>> TryLoadAsync(CancellationToken token = default)
        {
            if (IsValid == false) return default;

            try
            {
                var handle = Resources.LoadAsync<T>(Value.Value);

                if (handle == null)
                {
                    return default;
                }

                while (handle.isDone == false)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await Awaitable.NextFrameAsync(token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                if (token.IsCancellationRequested)
                {
                    return default;
                }

                var obj = handle.asset;

                if (obj is T asset && asset)
                {
                    return asset;
                }
            }
            catch
            {
                // ignored
            }

            return default;
        }
    }
}

#endif
