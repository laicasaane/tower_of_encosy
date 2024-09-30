using System;

namespace Module.Core.Mvvm.ViewBinding
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class BindingPropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class BindingCommandAttribute : Attribute { }
}
