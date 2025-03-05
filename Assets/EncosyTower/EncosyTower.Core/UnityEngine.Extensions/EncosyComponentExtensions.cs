using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    public static partial class EncosyComponentExtensions
    {
        public static T GetOrAddComponent<T>([NotNull] this Component self) where T : Component
        {
            ThrowIfComponentInvalid(self);

            if (self.TryGetComponent(out T component) == false)
            {
                component = self.gameObject.AddComponent<T>();
            }

            return component;
        }

        public static void DetachParent([NotNull] this Transform self, bool worldPositionStays = true)
        {
            ThrowIfComponentInvalid(self);
            self.SetParent(null, worldPositionStays);
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
