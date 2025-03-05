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

    public static partial class EncosyComponentExtensions
    {
        /// <summary>
        /// .enabled = false => 1 frame => enabled = true;
        /// </summary>
        public static async UnityTask EnableAsync([NotNull] this Behaviour self, CancellationToken token = default)
        {
            ThrowIfComponentInvalid(self);

            self.enabled = false;

            await UnityTasks.NextFrameAsync(token);

            self.enabled = true;
        }

        /// <summary>
        /// .enabled = false => N frame => enabled = true;
        /// </summary>
        public static async UnityTask EnableAsync([NotNull] this Behaviour self, int delayFrames, CancellationToken token = default)
        {
            ThrowIfComponentInvalid(self);

            self.enabled = false;

            for (var i = 0; i < delayFrames; i++)
            {
                await UnityTasks.NextFrameAsync(token);
            }

            self.enabled = true;
        }
    }
}

#endif
