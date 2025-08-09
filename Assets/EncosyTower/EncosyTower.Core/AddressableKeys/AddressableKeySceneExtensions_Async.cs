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
    using UnityTaskOpt = Cysharp.Threading.Tasks.UniTask<Option<SceneInstance>>;
#else
    using UnityTask = UnityEngine.Awaitable<SceneInstance>;
    using UnityTaskOpt = UnityEngine.Awaitable<Option<SceneInstance>>;
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
            var result = await TryLoadAsyncInternal(key, mode, activateOnLoad, priority, token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTaskOpt TryLoadAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            return TryLoadAsyncInternal(key, mode, activateOnLoad, priority, token);
        }

        private static async UnityTaskOpt TryLoadAsyncInternal(
              AddressableKey<Scene> key
            , LoadSceneMode mode
            , bool activateOnLoad
            , int priority
            , CancellationToken token
        )
        {
            if (key.IsValid == false) return default;

            var handle = Addressables.LoadSceneAsync(key.Value.Value, mode, activateOnLoad, priority);

            if (handle.IsValid() == false)
            {
                return default;
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
                return default;
            }

            return handle.Status != AsyncOperationStatus.Succeeded
                ? default(Option<SceneInstance>)
                : handle.Result;
        }
    }
}

#endif
#endif
