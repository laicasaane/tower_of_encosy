#pragma warning disable

namespace System;

public class Object
{
}

public abstract class Attribute
{
}

public sealed class AttributeUsageAttribute : Attribute
{
    public bool AllowMultiple { get; set; }

    public bool Inherited { get; set; }

    public AttributeUsageAttribute(AttributeTargets validOn)
    {
    }
}

[AttributeUsage(AttributeTargets.Enum, Inherited = false)]
public sealed class FlagsAttribute : Attribute
{
}

[Flags]
public enum AttributeTargets
{
    Assembly = 0x0001,
    Module = 0x0002,
    Class = 0x0004,
    Struct = 0x0008,
    Enum = 0x0010,
    Constructor = 0x0020,
    Method = 0x0040,
    Property = 0x0080,
    Field = 0x0100,
    Event = 0x0200,
    Interface = 0x0400,
    Parameter = 0x0800,
    Delegate = 0x1000,
    ReturnValue = 0x2000,
    GenericParameter = 0x4000,

    All = Assembly | Module | Class | Struct | Enum | Constructor |
                        Method | Property | Field | Event | Interface | Parameter |
                        Delegate | ReturnValue | GenericParameter
}

public interface IDisposable
{
    void Dispose();
}


public readonly struct ArraySegment<T>
{

}

#if NETSTANDARD2_1_OR_GREATER
public readonly ref struct Span<T>
{
    public Span(T[] array)
    {
    }

    public Span(T[] array, int start, int length)
    {
    }
}

public readonly ref struct ReadOnlySpan<T>
{
    public ReadOnlySpan(T[] array)
    {
    }

    public ReadOnlySpan(T[] array, int start, int length)
    {
    }
}

public readonly struct Memory<T>
{
    public Memory(T[] array)
    {
    }

    public Memory(T[] array, int start, int length)
    {
    }
}

public readonly struct ReadOnlyMemory<T>
{
    public ReadOnlyMemory(T[] array)
    {
    }

    public ReadOnlyMemory(T[] array, int start, int length)
    {
    }
}
#endif

public abstract class ValueType
{
}

public abstract class Enum : ValueType
{
}

public struct RuntimeTypeHandle
{
}

public abstract class Type
{
    public bool IsPrimitive { get => default; }
    public bool IsEnum { get => default; }
    public static Type? GetTypeFromHandle(RuntimeTypeHandle handle) => default;
}

public abstract class Array
{
    public static void Clear(Array array, int index, int length) { }
}

public class Exception
{
}

public class NullReferenceException : Exception
{
}

public class ArgumentOutOfRangeException : Exception
{
    public ArgumentOutOfRangeException(string? paramName, string? message) { }
}

public class NotImplementedException : Exception
{
}

public sealed class String
{
}

public struct Void
{
}

public struct Boolean
{
}

public struct Char
{
}

public struct Byte
{
}

public struct SByte
{
}

public struct UInt16
{
}

public struct Int16
{
}

public struct UInt32
{
}

public struct Int32
{
}

public struct UInt64
{
}

public struct Int64
{
}
