#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    public static partial class EncosyComponentExtensions
    {
        /// <summary>
        /// .enabled = false => 1 frame => enabled = true;
        /// </summary>
        public static async Awaitable EnableAsync([NotNull] this Behaviour self)
        {
            ThrowIfComponentInvalid(self);

            self.enabled = false;

            await Awaitable.EndOfFrameAsync();

            self.enabled = true;
        }

        /// <summary>
        /// .enabled = false => N frame => enabled = true;
        /// </summary>
        public static async Awaitable EnableAsync([NotNull] this Behaviour self, int delayFrames)
        {
            ThrowIfComponentInvalid(self);

            self.enabled = false;

            for (var i = 0; i < delayFrames; i++)
            {
                await Awaitable.EndOfFrameAsync();
            }

            self.enabled = true;
        }
    }
}

#endif
