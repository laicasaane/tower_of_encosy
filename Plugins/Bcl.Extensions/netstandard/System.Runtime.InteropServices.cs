#pragma warning disable

using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class InAttribute : Attribute
{
}

public static class MemoryMarshal
{
    public static ref T GetArrayDataReference<T>(T[] array)
    {
        throw new NotImplementedException();
    }

    public unsafe static ref byte GetArrayDataReference(Array array)
    {
        throw new NotImplementedException();
    }

    public static Span<byte> AsBytes<T>(Span<T> span) where T : struct
    {
        throw new NotImplementedException();
    }

    public static ReadOnlySpan<byte> AsBytes<T>(ReadOnlySpan<T> span) where T : struct
    {
        throw new NotImplementedException();
    }

    public static Memory<T> AsMemory<T>(ReadOnlyMemory<T> memory)
    {
        throw new NotImplementedException();
    }

    public static ref T GetReference<T>(Span<T> span)
    {
        throw new NotImplementedException();
    }

    public static ref T GetReference<T>(ReadOnlySpan<T> span)
    {
        throw new NotImplementedException();
    }

    internal unsafe static ref T GetNonNullPinnableReference<T>(Span<T> span)
    {
        throw new NotImplementedException();
    }

    internal unsafe static ref T GetNonNullPinnableReference<T>(ReadOnlySpan<T> span)
    {
        throw new NotImplementedException();
    }

    public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> span) where TFrom : struct where TTo : struct
    {
        throw new NotImplementedException();
    }

    public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(ReadOnlySpan<TFrom> span) where TFrom : struct where TTo : struct
    {
        throw new NotImplementedException();
    }

    public static Span<T> CreateSpan<T>(ref T reference, int length)
    {
        throw new NotImplementedException();
    }

    public static ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length)
    {
        throw new NotImplementedException();
    }

    public unsafe static ReadOnlySpan<char> CreateReadOnlySpanFromNullTerminated(char* value)
    {
        throw new NotImplementedException();
    }

    public unsafe static ReadOnlySpan<byte> CreateReadOnlySpanFromNullTerminated(byte* value)
    {
        throw new NotImplementedException();
    }

    public static bool TryGetArray<T>(ReadOnlyMemory<T> memory, out ArraySegment<T> segment)
    {
        throw new NotImplementedException();
    }

    public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, [NotNullWhen(true)] out TManager? manager) where TManager : MemoryManager<T>
    {
        throw new NotImplementedException();
    }

    public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, [NotNullWhen(true)] out TManager? manager, out int start, out int length) where TManager : MemoryManager<T>
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<T> ToEnumerable<T>(ReadOnlyMemory<T> memory)
    {
        throw new NotImplementedException();
    }

    public static bool TryGetString(ReadOnlyMemory<char> memory, [NotNullWhen(true)] out string? text, out int start, out int length)
    {
        throw new NotImplementedException();
    }

    public static T Read<T>(ReadOnlySpan<byte> source) where T : struct
    {
        throw new NotImplementedException();
    }

    public static bool TryRead<T>(ReadOnlySpan<byte> source, out T value) where T : struct
    {
        throw new NotImplementedException();
    }

    public static void Write<T>(Span<byte> destination, ref T value) where T : struct
    {
        throw new NotImplementedException();
    }

    public static bool TryWrite<T>(Span<byte> destination, ref T value) where T : struct
    {
        throw new NotImplementedException();
    }

    public static ref T AsRef<T>(Span<byte> span) where T : struct
    {
        throw new NotImplementedException();
    }

    public static ref readonly T AsRef<T>(ReadOnlySpan<byte> span) where T : struct
    {
        throw new NotImplementedException();
    }

    public static Memory<T> CreateFromPinnedArray<T>(T[]? array, int start, int length)
    {
        throw new NotImplementedException();
    }
}
