#if UNITASK

using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EncosyTower.Modules
{
    public static partial class EncosyGameObjectExtensions
    {
        /// <summary>
        /// SetActive(false) => 1 frame => SetActive(true);
        /// </summary>
        public static async UniTask ActivateAsync([NotNull] this GameObject self)
        {
            ThrowIfGameObjectInvalid(self);

            self.SetActive(false);

            await UniTask.NextFrame();

            self.SetActive(true);
        }

        /// <summary>
        /// SetActive(false) => N frame => SetActive(true);
        /// </summary>
        public static async UniTask ActivateAsync([NotNull] this GameObject self, int delayFrames)
        {
            ThrowIfGameObjectInvalid(self);

            self.SetActive(false);

            for (var i = 0; i < delayFrames; i++)
            {
                await UniTask.NextFrame();
            }

            self.SetActive(true);
        }
    }
}

#endif
