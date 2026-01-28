#pragma warning disable

namespace System.Security.Permissions;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public abstract partial class SecurityAttribute : Attribute
{
    protected SecurityAttribute(SecurityAction action)
    {
    }
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public abstract partial class CodeAccessSecurityAttribute : SecurityAttribute
{
    protected CodeAccessSecurityAttribute(SecurityAction action)
        : base(default)
    {
    }
}

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed partial class SecurityPermissionAttribute : CodeAccessSecurityAttribute
{
    public bool SkipVerification { get; set; }

    public SecurityPermissionAttribute(SecurityAction action)
        : base(default)
    {
    }
}

public enum SecurityAction
{
    RequestMinimum = 8
}
