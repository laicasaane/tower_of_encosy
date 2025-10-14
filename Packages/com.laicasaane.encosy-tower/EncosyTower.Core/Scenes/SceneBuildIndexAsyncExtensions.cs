#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Tasks;
using UnityEngine;
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

    public static partial class SceneBuildIndexAsyncExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask LoadAsync(
              this SceneBuildIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            var result = await TryLoadAsyncInternal(index, mode, token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTaskOpt TryLoadAsync(
              this SceneBuildIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            return TryLoadAsyncInternal(index, mode, token);
        }

        private static async UnityTaskOpt TryLoadAsyncInternal(
              SceneBuildIndex index
            , LoadSceneMode mode
            , CancellationToken token
        )
        {
#if UNITY_EDITOR
            if (Editor.Scenes.SceneBuildIndexEditorAPI.Validate(index) == false)
            {
                LogErrorIfInvalidInEditor(index);
                return Option.None;
            }
#else
            if (index.IsValid == false) return Option.None;
#endif

            var handle =  SceneManager.LoadSceneAsync(index.Index, mode);

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

            return token.IsCancellationRequested
                ? Option.None
                : SceneManager.GetSceneByBuildIndex(index.Index);
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorIfInvalidInEditor(SceneBuildIndex index)
        {
            StaticDevLogger.LogError(
                $"Cannot find scene with build index {index.Index} and name '{index.Name}' " +
                $"in the current EditorBuildSettings."
            );
        }
    }
}

#endif
#endif
