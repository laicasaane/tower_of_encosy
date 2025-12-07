using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    public static class EncosyUnityObjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this UnityEngine.Object self)
            => self != null && self != false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInvalid(this UnityEngine.Object self)
            => self == null || self == false;

        /// <summary>
        /// Prevent warning CA1062 if the object is surely valid but the compiler still complains.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AssumeValid<T>(this T self) where T : UnityEngine.Object
        {
            ThrowIfInvalid(self);
            return self;
        }

        [HideInCallstack, StackTraceHidden, DoesNotReturn, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfInvalid(UnityEngine.Object self)
        {
            if (self.IsInvalid())
            {
                throw new ArgumentNullException(nameof(self));
            }
        }
    }
}
