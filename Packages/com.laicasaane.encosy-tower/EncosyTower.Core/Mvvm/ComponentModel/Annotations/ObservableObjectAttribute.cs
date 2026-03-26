using System;

namespace EncosyTower.Mvvm.ComponentModel
{
    /// <summary>
    /// Any class annotated with this attribute will have its details generated
    /// by a source generator to implement the <see cref="IObservableObject"/> interface.
    /// </summary>
    /// <remarks>
    /// For performance reasons, the attribute is not inheritable.
    /// Each class that needs to be observable must be directly annotated with this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ObservableObjectAttribute : Attribute { }
}
