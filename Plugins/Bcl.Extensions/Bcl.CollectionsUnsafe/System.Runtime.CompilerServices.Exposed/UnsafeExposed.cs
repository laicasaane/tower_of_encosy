namespace System.Runtime.CompilerServices.Exposed;

using Unsafe = Internal.Runtime.CompilerServices.Unsafe;

internal static class UnsafeExposed
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void* AsPointer<T>(ref T value)
    {
        return Unsafe.AsPointer(ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOf<T>()
    {
        return Unsafe.SizeOf<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T As<T>(object? value) where T : class?
    {
        return Unsafe.As<T>(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TTo As<TFrom, TTo>(ref TFrom source)
    {
        return ref Unsafe.As<TFrom, TTo>(ref source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Add<T>(ref T source, int elementOffset)
    {
        return ref Unsafe.Add(ref source, elementOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Add<T>(ref T source, IntPtr elementOffset)
    {
        return ref Unsafe.Add(ref source, elementOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void* Add<T>(void* source, int elementOffset)
    {
        return Unsafe.Add<T>(source, elementOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreSame<T>([AllowNull] ref T left, [AllowNull] ref T right)
    {
        return Unsafe.AreSame(ref left, ref right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddressGreaterThan<T>([AllowNull] ref T left, [AllowNull] ref T right)
    {
        return Unsafe.IsAddressGreaterThan(ref left, ref right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddressLessThan<T>([AllowNull] ref T left, [AllowNull] ref T right)
    {
        return Unsafe.IsAddressLessThan(ref left, ref right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
    {
        Unsafe.InitBlockUnaligned(ref startAddress, value, byteCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static T ReadUnaligned<T>(void* source)
    {
        return Unsafe.ReadUnaligned<T>(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadUnaligned<T>(ref byte source)
    {
        return Unsafe.ReadUnaligned<T>(ref source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void WriteUnaligned<T>(void* destination, T value)
    {
        Unsafe.WriteUnaligned(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnaligned<T>(ref byte destination, T value)
    {
        Unsafe.WriteUnaligned(ref destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
    {
        return ref Unsafe.AddByteOffset(ref source, byteOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static T Read<T>(void* source)
    {
        return Unsafe.Read<T>(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(ref byte source)
    {
        return Unsafe.Read<T>(ref source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void Write<T>(void* destination, T value)
    {
        Unsafe.Write(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(ref byte destination, T value)
    {
        Unsafe.Write(ref destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static ref T AsRef<T>(void* source)
    {
        return ref Unsafe.AsRef<T>(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AsRef<T>(in T source)
    {
        return ref Unsafe.AsRef(in source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntPtr ByteOffset<T>([AllowNull] ref T origin, [AllowNull] ref T target)
    {
        return Unsafe.ByteOffset(ref origin, ref target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static ref T NullRef<T>()
    {
        return ref Unsafe.NullRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static bool IsNullRef<T>(ref T source)
    {
        return Unsafe.IsNullRef(ref source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipInit<T>(out T value)
    {
        Unsafe.SkipInit(out value);
    }
}
