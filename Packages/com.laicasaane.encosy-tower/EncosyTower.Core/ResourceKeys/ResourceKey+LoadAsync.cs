#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    partial record struct ResourceKey<T> : ILoadAsync<T>, ITryLoadAsync<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<T>
#else
            UnityEngine.Awaitable<T>
#endif
            LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadAsync(CancellationToken token = default)
        {
            if (IsValid == false) return Option.None;

            try
            {
                var handle = Resources.LoadAsync<T>(Value.Value);

                if (handle == null)
                {
                    return Option.None;
                }

                while (handle.isDone == false)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await UnityTasks.NextFrameAsync(token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                if (token.IsCancellationRequested)
                {
                    return Option.None;
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

            return Option.None;
        }
    }
}

#endif
