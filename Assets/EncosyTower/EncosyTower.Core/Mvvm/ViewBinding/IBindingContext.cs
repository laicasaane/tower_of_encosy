using System;
using EncosyTower.Mvvm.ComponentModel;

namespace EncosyTower.Mvvm.ViewBinding
{
    public interface IBindingContext
    {
        bool TryGetContext(out IObservableObject result);

        bool TryGetContextType(out Type result);
    }
}
