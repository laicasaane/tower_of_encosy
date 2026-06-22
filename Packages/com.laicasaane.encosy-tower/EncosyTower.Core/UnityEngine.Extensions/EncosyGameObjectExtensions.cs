using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.UnityExtensions
{
    public static partial class EncosyGameObjectExtensions
    {
        private const string CLONE_SUFFIX = "(Clone)";

        public static Component GetOrAddComponent([NotNull] this GameObject self, [NotNull] Type componentType)
        {
            ThrowIfGameObjectInvalid(self.IsValid());
            ThrowIfComponentTypeInvalid(IsComponentType(componentType), componentType);

            if (self.TryGetComponent(componentType, out var component) == false)
            {
                component = self.AddComponent(componentType);
            }

            return component;
        }

        public static T GetOrAddComponent<T>([NotNull] this GameObject self) where T : Component
        {
            ThrowIfGameObjectInvalid(self.IsValid());

            if (self.TryGetComponent(out T component) == false)
            {
                component = self.AddComponent<T>();
            }

            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToScene([NotNull] this GameObject self, Scene scene)
        {
            ThrowIfGameObjectInvalid(self.IsValid());
            ThrowIfSceneInvalid(scene.IsValid(), scene);
            SceneManager.MoveGameObjectToScene(self, scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToSceneWithoutParent([NotNull] this GameObject self, Scene scene)
        {
            ThrowIfGameObjectInvalid(self.IsValid());
            ThrowIfSceneInvalid(scene.IsValid(), scene);
            self.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(self, scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DetachParent([NotNull] this GameObject self, bool worldPositionStays = true)
        {
            ThrowIfGameObjectInvalid(self.IsValid());
            self.transform.SetParent(null, worldPositionStays);
        }

        /// <summary>
        /// Trim the "(Clone)" suffix usually added to the name of a GameObject when Instantiate is called.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static GameObject TrimCloneSuffix([NotNull] this GameObject self)
        {
            ThrowIfGameObjectInvalid(self.IsValid());

            var name = self.name.AsSpan();

            if (name.Length >= CLONE_SUFFIX.Length)
            {
                self.name = name[..^CLONE_SUFFIX.Length].ToString();
            }

            return self;
        }

        private static bool IsComponentType(Type type)
            => typeof(Component).IsAssignableFrom(type);

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfGameObjectInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("GameObject is null or invalid.", "self");
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfComponentTypeInvalid([DoesNotReturnIf(false)] bool isValid, Type type)
        {
            if (isValid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException(Type componentType)
                => new($"Type {componentType} is not a 'UnityEngine.Component'.", nameof(componentType));
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfSceneInvalid([DoesNotReturnIf(false)] bool isValid, Scene scene)
        {
            if (isValid == false)
            {
                throw CreateException(scene);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Scene scene)
                => new($"Scene {scene.handle} is invalid");
        }
    }
}
