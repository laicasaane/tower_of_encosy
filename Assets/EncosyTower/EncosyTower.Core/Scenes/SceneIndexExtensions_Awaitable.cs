#if UNITY_ADDRESSABLES
#if !UNITASK && UNITY_6000_0_OR_NEWER

namespace EncosyTower.Scenes
{
    using System.Runtime.CompilerServices;
    using System.Threading;
    using EncosyTower.Common;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static partial class SceneIndexExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Awaitable<Scene> LoadAsync(
              this SceneIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken  token = default
        )
        {
            var result = await TryLoadAsyncInternal(index, mode, token);
            return result.ValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<Option<Scene>> TryLoadAsync(
              this SceneIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            return TryLoadAsyncInternal(index, mode, token);
        }

        private static async Awaitable<Option<Scene>> TryLoadAsyncInternal(
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

                await Awaitable.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            return token.IsCancellationRequested
                ? default
                : SceneManager.GetSceneByBuildIndex(index.Index);
        }

        #region SERIALIZABLE
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<Scene> LoadAsync(
              this SceneIndex.Serializable key
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            return LoadAsync((SceneIndex)key, mode, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<Option<Scene>> TryLoadAsync(
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
