namespace System.Runtime.CompilerServices;

public static class RuntimeHelpers
{
#if NETSTANDARD2_1_OR_GREATER
	public static bool IsReferenceOrContainsReferences<T>() => default;
#endif
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
public sealed class MethodImplAttribute : Attribute
{
	public MethodImplAttribute(MethodImplOptions methodImplOptions) { }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class InternalsVisibleToAttribute : Attribute
{
    public InternalsVisibleToAttribute(string assemblyName)
    {
        AssemblyName = assemblyName;
    }

    public string AssemblyName { get; }
    public bool AllInternalsVisible { get; set; } = true;
}

[Flags]
public enum MethodImplOptions
{
	Unmanaged = 0x0004,
	NoInlining = 0x0008,
	ForwardRef = 0x0010,
	Synchronized = 0x0020,
	NoOptimization = 0x0040,
	PreserveSig = 0x0080,
	AggressiveInlining = 0x0100,
	AggressiveOptimization = 0x0200,
	InternalCall = 0x1000
}
