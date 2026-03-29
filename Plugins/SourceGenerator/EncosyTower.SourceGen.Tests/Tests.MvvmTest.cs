using System;
using System.Collections.Generic;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.Input;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.Mvvm.ViewBinding.Components;
using TMPro;
using UnityEngine;

namespace EncosyTower.Tests.MvvmTests
{
    [ObservableObject]
    public partial class Model : IObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Progress))]
        [field: UnityEngine.SerializeField]
        public int Value { get => Get_Value(); set => Set_Value(value); }

        public float Progress => Value / 100f;

        [RelayCommand]
        private void OnDoSomething() { }

        [RelayCommand]
        private void OnProcess(int value) { }
    }

    public struct A { }

    [Binder]
    public partial class Binder
    {
        [BindingProperty]
        [field: UnityEngine.HideInInspector]
        private void SetIntValue(int value)
        {
        }

        [BindingProperty]
        private void SetAValue(A value)
        {
        }

        [BindingProperty]
        private void SetListIntValue(List<int> value)
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

    [Binder]
    public partial class ExtendedBinder : Binder
    {
        [BindingProperty]
        private void SetExtendedIntValue(int value)
        {
        }
    }

    [ObservableObject]
    public partial class GenericModel<T> : IObservableObject
    {
        [ObservableProperty]
        public int Value { get => Get_Value(); set => Set_Value(value); }

        [RelayCommand]
        private void UpdateInt(int value)
        {
        }
    }

    [Binder]
    public partial class GenericBinder<T>
    {
        [BindingProperty]
        private void SetIntValue(int value)
        {
            Console.WriteLine(value);
        }
    }

    [MonoBinder(typeof(UnityEngine.GameObject))]
    [MonoBindingProperty(nameof(UnityEngine.GameObject.SetActive), UseCustomSetter = true)]
    public partial class GameObjectBinder
    {
        private static partial void Set_Active(GameObject target, bool value)
        {
            target.SetActive(value);
        }
    }

    [MonoBinder(typeof(UnityEngine.UI.Button))]
    [MonoBinderExcludeParent(typeof(UnityEngine.UI.Selectable))]
    public partial class UnityUIButtonBinder { }

    [MonoBinder(typeof(UnityEngine.BoxCollider2D), ExcludeObsolete = true)]
    public partial class UnityBoxCollider2DBinder { }

    [MonoBinder(typeof(TMPro.TMP_InputField))]
    [MonoBinderExcludeParent(typeof(UnityEngine.UI.Selectable))]
    [MonoBindingCommand(
          nameof(TMPro.TMP_InputField.onTextSelection)
        , WrapperType = typeof(TMPro.TMP_TextSelectionData)
        , Label = "On Text Selection"
    )]
    public partial class TMP_InputFieldBinder
    {
        private static partial void Unwrap_OnTextSelection(
            TMP_TextSelectionData value, out string p0, out int p1, out int p2
        )
        {
            (p0, p1, p2) = value;
        }

        private static partial TMP_TextSelectionData Wrap_OnTextSelection(string p0, int p1, int p2)
        {
            return new(p0, p1, p2);
        }
    }
}
