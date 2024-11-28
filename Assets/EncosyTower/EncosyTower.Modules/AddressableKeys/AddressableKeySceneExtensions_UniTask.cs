#if UNITY_ADDRESSABLES
#if UNITASK

namespace EncosyTower.Modules.AddressableKeys
{
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    public static partial class AddressableKeySceneExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<SceneInstance> LoadAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            var result = await TryLoadAsyncInternal(key, mode, activateOnLoad, priority, token);
            return result.ValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<SceneInstance>> TryLoadAsync(
              this AddressableKey<Scene> key
            , LoadSceneMode mode = LoadSceneMode.Single
            , bool activateOnLoad = true
            , int priority = 100
            , CancellationToken token = default
        )
        {
            return TryLoadAsyncInternal(key, mode, activateOnLoad, priority, token);
        }

        private static async UniTask<Option<SceneInstance>> TryLoadAsyncInternal(
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

                await UniTask.NextFrame(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            if (token.IsCancellationRequested)
            {
                return default;
            }

            return handle.Status != AsyncOperationStatus.Succeeded ? default : handle.Result;
        }
    }
}

#endif
#endif
