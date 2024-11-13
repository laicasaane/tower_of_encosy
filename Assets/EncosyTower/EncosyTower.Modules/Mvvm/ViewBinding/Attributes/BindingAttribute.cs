using System;

namespace EncosyTower.Modules.Mvvm.ViewBinding
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class BindingPropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class BindingCommandAttribute : Attribute { }
}
