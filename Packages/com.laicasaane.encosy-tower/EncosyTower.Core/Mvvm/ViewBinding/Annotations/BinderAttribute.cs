using System;

namespace EncosyTower.Mvvm.ViewBinding
{
    /// <summary>
    /// Any class annotated with this attribute will have its details generated
    /// by a source generator to implement the <see cref="IBinder"/> interface.
    /// </summary>
    /// <remarks>
    /// For performance reasons, the attribute is not inheritable.
    /// Each class that needs to be a binder must be directly annotated with this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BinderAttribute : Attribute { }
}
