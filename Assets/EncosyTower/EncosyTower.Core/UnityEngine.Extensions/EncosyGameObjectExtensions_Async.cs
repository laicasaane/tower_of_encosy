#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public static partial class EncosyGameObjectExtensions
    {
        /// <summary>
        /// SetActive(false) => 1 frame => SetActive(true);
        /// </summary>
        public static async UnityTask ActivateAsync([NotNull] this GameObject self, CancellationToken token = default)
        {
            ThrowIfGameObjectInvalid(self);

            self.SetActive(false);

            await UnityTasks.NextFrameAsync(token);

            self.SetActive(true);
        }

        /// <summary>
        /// SetActive(false) => N frame => SetActive(true);
        /// </summary>
        public static async UnityTask ActivateAsync([NotNull] this GameObject self, int delayFrames, CancellationToken token = default)
        {
            ThrowIfGameObjectInvalid(self);

            self.SetActive(false);

            for (var i = 0; i < delayFrames; i++)
            {
                await UnityTasks.NextFrameAsync(token);
            }

            self.SetActive(true);
        }
    }
}

#endif
