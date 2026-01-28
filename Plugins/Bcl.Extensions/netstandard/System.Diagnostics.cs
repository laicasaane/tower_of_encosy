#pragma warning disable

using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics;

public static class Debug
{
    public static void Assert([DoesNotReturnIf(false)] bool condition)
    {
        throw new NotImplementedException();
    }

    public static void Assert([DoesNotReturnIf(false)] bool condition, string? message)
    {
        throw new NotImplementedException();
    }
}
