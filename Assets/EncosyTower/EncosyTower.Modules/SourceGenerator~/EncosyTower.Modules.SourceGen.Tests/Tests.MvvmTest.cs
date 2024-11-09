using System;
using EncosyTower.Modules.Mvvm.ComponentModel;
using EncosyTower.Modules.Mvvm.Input;
using EncosyTower.Modules.Mvvm.ViewBinding;
using UnityEngine;

namespace EncosyTower.Modules.Tests.MvvmTests
{
    public partial class Model : IObservableObject
    {
        [NotifyPropertyChangedFor(nameof(Progress))]
        [ObservableProperty] public int Value { get => Get_Value(); set => Set_Value(value); }

        public float Progress => Value / 100f;

        [RelayCommand]
        private void OnDoSomething() { }

        [RelayCommand]
        private void OnProcess(int value) { }
    }

    public partial class Binder : IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetIntValue(int value)
        {
        }

        [BindingProperty]
        private void SetModelValue(Model value)
        {
        }

        [BindingProperty]
        private void SetTypeCode(ref TypeCode value)
        {
        }

        [BindingProperty]
        private void DoSomething()
        {

        }

        [BindingCommand]
        partial void OnValueChanged();

        [BindingCommand]
        partial void OnBoolValueChanged(ref bool value);
    }

    public partial class GenericModel<T> : IObservableObject
    {
        [ObservableProperty]
        public int Value { get => Get_Value(); set => Set_Value(value); }

        [RelayCommand]
        private void UpdateInt(int value)
        {
        }
    }

    public partial class GenericBinder<T> : IBinder
    {
        [BindingProperty]
        private void SetIntValue(int value)
        {
            Console.WriteLine(value);
        }
    }
}
