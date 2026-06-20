#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace EncosyTower.AddressableKeys
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask<SceneInstance>;
    using UnityTaskHandle = Cysharp.Threading.Tasks.UniTask<(SceneInstance, AsyncOperationHandle<SceneInstance>)>;
    using UnityTaskOpt = Cysharp.Threading.Tasks.UniTask<Option<SceneInstance>>;
    using UnityTaskHandleOpt = Cysharp.Threading.Tasks.UniTask<Option<(SceneInstance, AsyncOperationHandle<SceneInstance>)>>;
#else
    using UnityTask = UnityEngine.Awaitable<SceneInstance>;
    using UnityTaskHandle = UnityEngine.Awaitable<(SceneInstance, AsyncOperationHandle<SceneInstance>)>;
    using UnityTaskOpt = UnityEngine.Awaitable<Option<SceneInstance>>;
    using UnityTaskHandleOpt = UnityEngine.Awaitable<Option<(SceneInstance, AsyncOperationHandle<SceneInstance>)>>;
#endif

    public static partial class AddressableKeySceneExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask LoadAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await TryLoadGetHandleAsync(key, mode, activateOnLoad, priority, token);
            return result.GetValueOrDefault().Item1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTaskOpt TryLoadAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await TryLoadGetHandleAsync(key, mode, activateOnLoad, priority, token);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Item1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTaskHandle LoadGetHandleAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await TryLoadGetHandleAsync(key, mode, activateOnLoad, priority, token);
            return result.GetValueOrDefault();
        }

        public static async UnityTaskHandleOpt TryLoadGetHandleAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode
            , bool activateOnLoad
            , int priority
            , CancellationToken token
        )
        {
            if (key.IsValid == false) return Option.None;

            var handle = Addressables.LoadSceneAsync(key.Value.Value, mode, activateOnLoad, priority);

            if (handle.IsValid() == false)
            {
                handle.TryRelease();
                return Option.None;
            }

            while (handle.IsDone == false)
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
                handle.TryRelease();
                return Option.None;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return (handle.Result, handle);
            }

            handle.TryRelease();
            return Option.None;
        }
    }
}

#endif
#endif
