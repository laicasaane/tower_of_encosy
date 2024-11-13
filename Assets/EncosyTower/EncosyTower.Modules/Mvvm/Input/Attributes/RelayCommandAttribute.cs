using System;

namespace EncosyTower.Modules.Mvvm.Input
{
    /// <summary>
    /// An attribute that can be used to automatically generate <see cref="ICommand"/> properties from declared methods. When this attribute
    /// is used to decorate a method, a generator will create a command property with the corresponding <see cref="IRelayCommand"/> interface
    /// depending on the signature of the method. If an invalid method signature is used, the generator will report an error.
    /// <para>
    /// In order to use this attribute, the containing type doesn't need to implement any interfaces. The generated properties will be lazily
    /// assigned but their value will never change, so there is no need to support property change notifications or other additional functionality.
    /// </para>
    /// <para>
    /// This attribute can be used as follows:
    /// <code>
    /// partial class MyViewModel
    /// {
    ///     [RelayCommand]
    ///     private void GreetUser(User? user)
    ///     {
    ///         Console.WriteLine($"Hello {user.Name}!");
    ///     }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// The following signatures are supported for annotated methods:
    /// <code>
    /// void Method();
    /// </code>
    /// Will generate an <see cref="IRelayCommand"/> property (using a <see cref="RelayCommand"/> instance).
    /// <code>
    /// void Method(T);
    /// </code>
    /// Will generate an <see cref="IRelayCommand{T}"/> property (using a <see cref="RelayCommand{T}"/> instance).
    /// <code>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class RelayCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the property or method that will be invoked to check whether the
        /// generated command can be executed at any given time. The referenced member needs to return
        /// a <see cref="bool"/> value, and has to have a signature compatible with the target command.
        /// </summary>
        public string CanExecute { get; set; }
    }
}
