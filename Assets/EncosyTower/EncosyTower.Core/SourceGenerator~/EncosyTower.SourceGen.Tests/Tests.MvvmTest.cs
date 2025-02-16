using System;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.Input;
using EncosyTower.Mvvm.ViewBinding;

namespace EncosyTower.Tests.MvvmTests
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
        [field: UnityEngine.HideInInspector]
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
