#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace EncosyTower.Modules
{
    public static partial class EncosyGameObjectExtensions
    {
        /// <summary>
        /// SetActive(false) => 1 frame => SetActive(true);
        /// </summary>
        public static async Awaitable ActivateAsync([NotNull] this GameObject self)
        {
            ThrowIfGameObjectInvalid(self);

            self.SetActive(false);

            await Awaitable.EndOfFrameAsync();

            self.SetActive(true);
        }

        /// <summary>
        /// SetActive(false) => N frame => SetActive(true);
        /// </summary>
        public static async Awaitable ActivateAsync([NotNull] this GameObject self, int delayFrames)
        {
            ThrowIfGameObjectInvalid(self);

            self.SetActive(false);

            for (var i = 0; i < delayFrames; i++)
            {
                await Awaitable.EndOfFrameAsync();
            }

            self.SetActive(true);
        }
    }
}

#endif
