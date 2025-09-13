using EncosyTower.Variants;

namespace EncosyTower.Mvvm.Event
{
    public readonly struct MvvmEventArgs
    {
        public readonly object Sender;
        public readonly Variant Value;

        public MvvmEventArgs(object sender, in Variant value)
        {
            Sender = sender;
            Value = value;
        }
    }
}
