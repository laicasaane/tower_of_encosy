namespace EncosyTower.Modules
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static partial class CoreComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToScene([NotNull] this Component self, Scene scene)
        {
            ThrowIfComponentInvalid(self);
            self.gameObject.MoveToScene(scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToSceneWithoutParent([NotNull] this Component self, Scene scene)
        {
            ThrowIfComponentInvalid(self);
            self.gameObject.MoveToSceneWithoutParent(scene);
        }

        public static void DetachParent([NotNull] this Component self, bool worldPositionStays = true)
        {
            ThrowIfComponentInvalid(self);

            if (self is Transform transform)
            {
                transform.SetParent(null, worldPositionStays);
            }
            else
            {
                self.transform.SetParent(null, worldPositionStays);
            }
        }

        /// <summary>
        /// Trim the "(Clone)" suffix usually added to the name of a GameObject when Instantiate is called.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Component TrimCloneSuffix([NotNull] this Component self)
        {
            ThrowIfComponentInvalid(self);
            self.gameObject.TrimCloneSuffix();
            return self;
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfComponentInvalid(Component self)
        {
            if (self.IsInvalid())
            {
                throw new ArgumentNullException(nameof(self));
            }
        }
    }
}

#if UNITASK

namespace EncosyTower.Modules
{
    using System.Diagnostics.CodeAnalysis;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public static partial class CoreComponentExtensions
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
