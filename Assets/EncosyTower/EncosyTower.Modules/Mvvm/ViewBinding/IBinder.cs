using EncosyTower.Modules.Mvvm.ComponentModel;

namespace EncosyTower.Modules.Mvvm.ViewBinding
{
    /// <summary>
    /// Any class implements this interface will be an eligible
    /// candidate to have its details generated by the corresponding generator.
    /// </summary>
    /// <seealso cref="BindingPropertyAttribute"/>
    /// <seealso cref="BindingCommandAttribute"/>
    public interface IBinder
    {
        IObservableObject Context { get; }

        /// <summary>
        /// Sets target property name to a binding property.
        /// </summary>
        /// <param name="bindingPropertyName">The binding property whose <see cref="BindingProperty.TargetPropertyName"/> will be set.</param>
        /// <param name="targetPropertyName">
        /// The property whose container class is an <see cref="EncosyTower.Modules.Mvvm.ComponentModel.IObservableObject"/>.
        /// <br/>
        /// See <see cref="EncosyTower.Modules.Mvvm.ViewBinding.BindingProperty.TargetPropertyName"/>
        /// </param>
        public bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName) => false;

        /// <summary>
        /// Sets an instance of <see cref="IAdapter"/> to the converter of a binding property.
        /// </summary>
        /// <param name="bindingPropertyName">The binding property whose corresponding <see cref="Converter.Adapter"/> will be set.</param>
        /// <param name="adapter">An instance of <see cref="IAdapter"/>.</param>
        public bool SetAdapter(string bindingPropertyName, IAdapter adapter) => false;

        /// <summary>
        /// Sets target command name to a binding command.
        /// </summary>
        /// <param name="bindingCommandName">The binding command whose <see cref="BindingCommand.TargetCommandName"/> will be set.</param>
        /// <param name="targetCommandName">
        /// The command whose container class is an <see cref="EncosyTower.Modules.Mvvm.ComponentModel.IObservableObject"/>.
        /// <br/>
        /// See <see cref="EncosyTower.Modules.Mvvm.ViewBinding.BindingCommand.TargetCommandName"/>
        /// </param>
        public bool SetTargetCommandName(string bindingCommandName, string targetCommandName) => false;

        /// <summary>
        /// Start listening to events from the <see cref="Context"/>.
        /// </summary>
        public void StartListening() { }

        /// <summary>
        /// Stop listening to events from the <see cref="Context"/>.
        /// </summary>
        public void StopListening() { }

        /// <summary>
        /// Force the <see cref="Context"/> to send property changed notifications to the attached listeners.
        /// </summary>
        public void RefreshContext() { }
    }
}