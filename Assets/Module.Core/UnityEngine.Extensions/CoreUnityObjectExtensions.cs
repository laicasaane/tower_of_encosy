using System.Runtime.CompilerServices;

namespace Module.Core
{
    public static class CoreUnityObjectExtensions
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
        public static T AlwaysValid<T>(this T self) where T : UnityEngine.Object
            => self;
    }
}
