using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules
{
    public static class EncosyLayerMaskExtensions
    {
        /// <summary>
		/// Determines whether some of the bit fields are set in the current instance.
		/// </summary>
		/// <returns>
        /// <c>true</c> if all the bit fields that are set in <c>flag</c> are also set in the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this LayerMask value, LayerMask flag)
            => (value & flag) == flag;

        /// <summary>
        /// Determines whether the bit at <paramref name="bit"/> is set in the current instance.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the bit at <paramref name="bit"/> is set in the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this LayerMask value, int bit)
            => bit >= 0 && Contains(value, ToLayerMask(bit));

        /// <summary>
        /// Determines whether any of the bit fields are set in the current instance.
        /// </summary>
        /// <returns>
        /// <c>true</c> if any of the bit fields that are set in <c>flag</c> is also set in the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(this LayerMask value, LayerMask flag)
            => (value & flag) != 0;

        /// <summary>
        /// Unsets one or more bit fields on the current instance.
        /// </summary>
        /// <returns>A new instance without bit fields that are set in <c>flags</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayerMask Unset(this LayerMask value, LayerMask flag)
            => value & (~flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayerMask Unset(this LayerMask value, int bit)
            => bit >= 0 ? value & (~(1 << bit)) : value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayerMask GetLayerMask(this GameObject go)
            => go != null && go ? ToLayerMask(go.layer) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayerMask ToLayerMask(this int bit)
            => 1 << Mathf.Max(bit, 0);
    }
}
