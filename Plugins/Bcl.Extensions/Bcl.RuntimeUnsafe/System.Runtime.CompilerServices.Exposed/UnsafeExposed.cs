using EncosyTower.LowLevel.Unsafe;

namespace System.Runtime.CompilerServices.Exposed;

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
    public static bool AreSame<T>(ref T left, ref T right)
    {
        return Unsafe.AreSame(ref left, ref right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddressLessThan<T>(ref T left, ref T right)
    {
        return Unsafe.IsAddressLessThan(ref left, ref right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
    {
        Unsafe.InitBlockUnaligned(ref startAddress, value, byteCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadUnaligned<T>(ref byte source)
    {
        return Unsafe.ReadUnaligned<T>(ref source);
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
    public unsafe static ref T NullRef<T>()
    {
        return ref ILSupport.NullRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static bool IsNullRef<T>(in T source)
    {
        return ILSupport.IsNullRef(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipInit<T>(out T value)
    {
        ILSupport.SkipInit(out value);
    }
}
