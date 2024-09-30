using Module.Core.Mvvm.ComponentModel;

namespace Module.Core.Mvvm.ViewBinding
{
    public interface IBindingContext
    {
        bool IsCreated { get; }

        IObservableObject Target { get; }
    }
}
