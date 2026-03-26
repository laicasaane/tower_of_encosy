using System;

namespace EncosyTower.Mvvm.ViewBinding
{
    /// <summary>
    /// Specifies a method of an <see cref="IBinder"/> class as a binding property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class BindingPropertyAttribute : Attribute { }

    /// <summary>
    /// Specifies a method of an <see cref="IBinder"/> class as a binding command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class BindingCommandAttribute : Attribute { }
}
