using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules
{
    public static partial class EncosyComponentExtensions
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

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfComponentInvalid(Component self)
        {
            if (self.IsInvalid())
            {
                throw new ArgumentNullException(nameof(self));
            }
        }
    }
}
