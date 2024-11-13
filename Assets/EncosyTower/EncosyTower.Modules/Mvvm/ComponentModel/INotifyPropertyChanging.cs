namespace EncosyTower.Modules.Mvvm.ComponentModel
{
    /// <summary>
    /// Notifies clients that a property value is changing.
    /// </summary>
    public interface INotifyPropertyChanging
    {
        /// <summary>
        /// Attach an instance of <see cref="PropertyChangeEventListener{TInstance}"/> to an observable property.
        /// Before the value of that property is changed, a notification will be sent to this <paramref name="listener"/>.
        /// </summary>
        /// <typeparam name="TInstance">The owner of the <paramref name="listener"/>.</typeparam>
        /// <param name="propertyName">The observable property whose notifications will be listened to.</param>
        /// <param name="listener">The event listener to receive the notifications.</param>
        /// <see langword="true"/> if the specified property exists; otherwise <see langword="false"/>.
        bool AttachPropertyChangingListener<TInstance>(string propertyName, PropertyChangeEventListener<TInstance> listener)
            where TInstance : class;
    }
}
