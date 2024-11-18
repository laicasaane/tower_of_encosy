using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules
{
    public static partial class EncosyGameObjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToScene([NotNull] this GameObject self, Scene scene)
        {
            ThrowIfGameObjectInvalid(self);
            ThrowIfSceneInvalid(scene);
            SceneManager.MoveGameObjectToScene(self, scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToSceneWithoutParent([NotNull] this GameObject self, Scene scene)
        {
            ThrowIfGameObjectInvalid(self);
            ThrowIfSceneInvalid(scene);
            self.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(self, scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DetachParent([NotNull] this GameObject self, bool worldPositionStays = true)
        {
            ThrowIfGameObjectInvalid(self);
            self.transform.SetParent(null, worldPositionStays);
        }

        /// <summary>
        /// Trim the "(Clone)" suffix usually added to the name of a GameObject when Instantiate is called.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static GameObject TrimCloneSuffix([NotNull] this GameObject self)
        {
            ThrowIfGameObjectInvalid(self);

            const string SUFFIX = "(Clone)";
            var name = self.name.AsSpan();

            if (name.Length >= SUFFIX.Length)
            {
                self.name = name[..^SUFFIX.Length].ToString();
            }

            return self;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfGameObjectInvalid(GameObject self)
        {
            if (self.IsInvalid())
            {
                throw new ArgumentNullException(nameof(self));
            }
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfSceneInvalid(Scene scene)
        {
            if (scene.IsValid() == false)
            {
                throw new InvalidOperationException($"Scene {scene.handle} is invalid");
            }
        }
    }
}
