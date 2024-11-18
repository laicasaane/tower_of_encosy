#if UNITASK

using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EncosyTower.Modules
{
    public static partial class EncosyComponentExtensions
    {
        /// <summary>
        /// SetActive(false) => 1 frame => SetActive(true);
        /// </summary>
        public static UniTask ActivateAsync([NotNull] this Component self)
        {
            ThrowIfComponentInvalid(self);
            return self.gameObject.ActivateAsync();
        }

        /// <summary>
        /// SetActive(false) => N frame => SetActive(true);
        /// </summary>
        public static UniTask ActivateAsync([NotNull] this Component self, int delayFrames)
        {
            ThrowIfComponentInvalid(self);
            return self.ActivateAsync(delayFrames);
        }

        /// <summary>
        /// .enabled = false => 1 frame => enabled = true;
        /// </summary>
        public static async UniTask EnableAsync([NotNull] this Behaviour self)
        {
            ThrowIfComponentInvalid(self);

            self.enabled = false;

            await UniTask.NextFrame();

            self.enabled = true;
        }

        /// <summary>
        /// .enabled = false => N frame => enabled = true;
        /// </summary>
        public static async UniTask EnableAsync([NotNull] this Behaviour self, int delayFrames)
        {
            ThrowIfComponentInvalid(self);

            self.enabled = false;

            for (var i = 0; i < delayFrames; i++)
            {
                await UniTask.NextFrame();
            }

            self.enabled = true;
        }
    }
}

#endif
