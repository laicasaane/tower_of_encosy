namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class IgnoresAccessChecksToAttribute : Attribute
{
    public string AssemblyName { get; }

    public IgnoresAccessChecksToAttribute(string assemblyName)
    {
        AssemblyName = assemblyName;
    }
}
