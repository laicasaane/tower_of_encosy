namespace EncosyTower.Modules.Mvvm.ComponentModel
{
    /// <summary>
    /// Notifies clients that a property value has changed.
    /// </summary>
    public interface INotifyPropertyChanged
    {
        /// <summary>
        /// Attach an instance of <see cref="PropertyChangeEventListener{TInstance}"/> to an observable property.
        /// After the value of that property is changed, a notification will be sent to this <paramref name="listener"/>.
        /// </summary>
        /// <typeparam name="TInstance">The owner of the <paramref name="listener"/>.</typeparam>
        /// <param name="propertyName">The observable property whose notifications will be listened to.</param>
        /// <param name="listener">The event listener to receive the notifications.</param>
        /// <see langword="true"/> if the specified property exists; otherwise <see langword="false"/>.
        bool AttachPropertyChangedListener<TInstance>(string propertyName, PropertyChangeEventListener<TInstance> listener)
            where TInstance : class;

        /// <summary>
        /// Force an observable property to send its current value to <paramref name="listener"/>.
        /// </summary>
        /// <typeparam name="TInstance">The owner of the <paramref name="listener"/>.</typeparam>
        /// <param name="propertyName">The observable property to send its current value.</param>
        /// <param name="listener">The event listener to receive the notifications.</param>
        /// <returns>
        /// <see langword="true"/> if the specified property exists; otherwise <see langword="false"/>.
        /// </returns>
        bool NotifyPropertyChanged<TInstance>(string propertyName, PropertyChangeEventListener<TInstance> listener)
            where TInstance : class;

        /// <summary>
        /// Force an observable property to send its current value to the attached listeners.
        /// </summary>
        /// <param name="propertyName">The observable property to send its current value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified property exists; otherwise <see langword="false"/>.
        /// </returns>
        bool NotifyPropertyChanged(string propertyName);

        /// <summary>
        /// Force all observable properties to send their current value to the attached listeners.
        /// </summary>
        void NotifyPropertyChanged();
    }
}
