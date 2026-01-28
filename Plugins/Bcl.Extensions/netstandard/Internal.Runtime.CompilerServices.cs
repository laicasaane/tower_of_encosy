#pragma warning disable

using System;

namespace Internal.Runtime.CompilerServices;

public static class Unsafe
{
    public unsafe static void* AsPointer<T>(ref T value)
    {
        throw new NotImplementedException();
    }

    public static int SizeOf<T>()
    {
        throw new NotImplementedException();
    }

    public static T As<T>(object? value) where T : class?
    {
        throw new NotImplementedException();
    }

    public static ref TTo As<TFrom, TTo>(ref TFrom source)
    {
        throw new NotImplementedException();
    }

    public static ref T Add<T>(ref T source, int elementOffset)
    {
        throw new NotImplementedException();
    }

    public static ref T Add<T>(ref T source, IntPtr elementOffset)
    {
        throw new NotImplementedException();
    }

    public unsafe static void* Add<T>(void* source, int elementOffset)
    {
        throw new NotImplementedException();
    }

    public static bool AreSame<T>([AllowNull] ref T left, [AllowNull] ref T right)
    {
        throw new NotImplementedException();
    }

    public static bool IsAddressGreaterThan<T>([AllowNull] ref T left, [AllowNull] ref T right)
    {
        throw new NotImplementedException();
    }

    public static bool IsAddressLessThan<T>([AllowNull] ref T left, [AllowNull] ref T right)
    {
        throw new NotImplementedException();
    }

    public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
    {
        throw new NotImplementedException();
    }

    public unsafe static T ReadUnaligned<T>(void* source)
    {
        throw new NotImplementedException();
    }

    public static T ReadUnaligned<T>(ref byte source)
    {
        throw new NotImplementedException();
    }

    public unsafe static void WriteUnaligned<T>(void* destination, T value)
    {
        throw new NotImplementedException();
    }

    public static void WriteUnaligned<T>(ref byte destination, T value)
    {
        throw new NotImplementedException();
    }

    public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
    {
        throw new NotImplementedException();
    }

    public unsafe static T Read<T>(void* source)
    {
        throw new NotImplementedException();
    }

    public static T Read<T>(ref byte source)
    {
        throw new NotImplementedException();
    }

    public unsafe static void Write<T>(void* destination, T value)
    {
        throw new NotImplementedException();
    }

    public static void Write<T>(ref byte destination, T value)
    {
        throw new NotImplementedException();
    }

    public unsafe static ref T AsRef<T>(void* source)
    {
        throw new NotImplementedException();
    }

    public static ref T AsRef<T>(in T source)
    {
        throw new NotImplementedException();
    }

    public static IntPtr ByteOffset<T>([AllowNull] ref T origin, [AllowNull] ref T target)
    {
        throw new NotImplementedException();
    }

    public unsafe static ref T NullRef<T>()
    {
        throw new NotImplementedException();
    }

    public unsafe static bool IsNullRef<T>(ref T source)
    {
        throw new NotImplementedException();
    }

    public static void SkipInit<T>(out T value)
    {
        throw new NotImplementedException();
    }
}
