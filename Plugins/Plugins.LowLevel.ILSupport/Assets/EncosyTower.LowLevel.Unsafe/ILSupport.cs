using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.LowLevel.Unsafe
{
    internal unsafe static class ILSupport
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AddressOf<T>(in T thing)
        {
            throw new NotImplementedException();

            //ldarg .0
            //ret
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(in T thing)
        {
            throw new NotImplementedException();

            //ldarg .0
            //ret
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T NullRef<T>()
        {
            throw new NotImplementedException();

            // ldc.i4.0
            // conv.u
            // ret
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullRef<T>(in T source)
        {
            throw new NotImplementedException();

            // ldarg.0
            // ldc.i4.0
            // conv.u
            // ceq
            // ret
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipInit<T>(out T value)
        {
            throw new NotImplementedException();

            // ret
        }
    }
}
