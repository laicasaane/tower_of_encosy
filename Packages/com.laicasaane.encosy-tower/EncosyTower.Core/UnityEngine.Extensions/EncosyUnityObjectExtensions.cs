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
            ThrowIfObjectInvalid(self.IsValid());
            return self;
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfObjectInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("UnityEngine.Object is null or invalid.", "self");
        }
    }
}
