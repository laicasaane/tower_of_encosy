#pragma warning disable

namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class TargetFrameworkAttribute : Attribute
{
    public string FrameworkDisplayName { get; set; }

    public string FrameworkName { get; }

    public TargetFrameworkAttribute(string frameworkName)
    {
    }
}
