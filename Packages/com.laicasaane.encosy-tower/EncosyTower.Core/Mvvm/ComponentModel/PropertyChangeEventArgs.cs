using EncosyTower.Variants;

namespace EncosyTower.Mvvm.ComponentModel
{
    public readonly struct PropertyChangeEventArgs
    {
        public readonly IObservableObject Sender;
        public readonly string PropertyName;
        public readonly Variant OldValue;
        public readonly Variant NewValue;

        public PropertyChangeEventArgs(IObservableObject sender, string propertyName, in Variant oldValue, in Variant newValue)
        {
            Sender = sender;
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
