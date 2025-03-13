#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;
using UnityEngine.SceneManagement;

namespace EncosyTower.Scenes
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask<Scene>;
    using UnityTaskOpt = Cysharp.Threading.Tasks.UniTask<Option<Scene>>;
#else
    using UnityTask = UnityEngine.Awaitable<Scene>;
    using UnityTaskOpt = UnityEngine.Awaitable<Option<Scene>>;
#endif

    public static partial class SceneIndexExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask LoadAsync(
              this SceneIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            var result = await TryLoadAsyncInternal(index, mode, token);
            return result.ValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTaskOpt TryLoadAsync(
              this SceneIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            return TryLoadAsyncInternal(index, mode, token);
        }

        private static async UnityTaskOpt TryLoadAsyncInternal(
              SceneIndex index
            , LoadSceneMode mode
            , CancellationToken token
        )
        {
            if (index.IsValid == false) return default;

            var handle =  SceneManager.LoadSceneAsync(index.Index, mode);

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

                await UnityTasks.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            return token.IsCancellationRequested
                ? default(Option<Scene>)
                : SceneManager.GetSceneByBuildIndex(index.Index);
        }

        #region SERIALIZABLE
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask LoadAsync(
              this SceneIndex.Serializable key
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            return LoadAsync((SceneIndex)key, mode, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTaskOpt TryLoadAsync(
              this SceneIndex.Serializable key
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            return TryLoadAsync((SceneIndex)key, mode, token);
        }
    }
}

#endif
#endif
