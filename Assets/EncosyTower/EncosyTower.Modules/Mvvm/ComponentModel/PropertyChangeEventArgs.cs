using EncosyTower.Modules.Unions;

namespace EncosyTower.Modules.Mvvm.ComponentModel
{
    public readonly struct PropertyChangeEventArgs
    {
        public readonly IObservableObject Sender;
        public readonly string PropertyName;
        public readonly Union OldValue;
        public readonly Union NewValue;

        public PropertyChangeEventArgs(IObservableObject sender, string propertyName, in Union oldValue, in Union newValue)
        {
            Sender = sender;
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
