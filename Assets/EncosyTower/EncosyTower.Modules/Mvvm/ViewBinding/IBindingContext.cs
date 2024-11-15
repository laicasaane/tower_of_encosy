using System;
using EncosyTower.Modules.Mvvm.ComponentModel;

namespace EncosyTower.Modules.Mvvm.ViewBinding
{
    public interface IBindingContext
    {
        bool TryGetContext(out IObservableObject result);

        bool TryGetContextType(out Type result);
    }
}
