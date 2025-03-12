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

        public static void FillParent([NotNull] this RectTransform self, [NotNull] RectTransform parent)
        {
            ThrowIfComponentInvalid(self);
            ThrowIfComponentInvalid(parent);

            self.SetParent(parent, false);
            self.localPosition = Vector3.zero;
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.one;
            self.offsetMin = Vector2.zero;
            self.offsetMax = Vector2.zero;
            self.pivot = new Vector2(0.5f, 0.5f);
            self.rotation = Quaternion.identity;
            self.localScale = Vector3.one;
        }

        public static void SetParentRect([NotNull] this RectTransform self, [NotNull] RectTransform parent)
        {
            ThrowIfComponentInvalid(self);
            ThrowIfComponentInvalid(parent);

            self.SetParent(parent, false);
            self.localPosition = Vector3.zero;
            self.rotation = Quaternion.identity;
            self.localScale = Vector3.one;
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
