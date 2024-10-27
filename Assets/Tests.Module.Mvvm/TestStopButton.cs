using System;
using Module.Core;
using Module.Core.Mvvm.ComponentModel;
using Module.Core.Mvvm.Input;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;
using UnityEngine;

namespace Tests.Module.Mvvm
{
    public sealed partial class TestStopButton : MonoBehaviour, IObservableObject
    {
        [ObservableProperty]
        public bool Stopped { get => Get_Stopped(); set => Set_Stopped(value); }

        [RelayCommand]
        private void OnClick()
        {
            Stopped = !Stopped;
        }
    }

    [Serializable]
    [Label("Start/Stop ⇒ Color", "Default")]
    [Adapter(sourceType: typeof(bool), destType: typeof(Color), order: 0)]
    public sealed class StartStopColorAdapter : IAdapter
    {
        [SerializeField] private Color _start = Color.white;
        [SerializeField] private Color _stop = Color.white;
        [SerializeField] private bool _invert = false;

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out bool result))
            {
                return ActualValue(result) ? new ColorUnion(_stop) : new ColorUnion(_start);
            }

            return union;
        }

        private bool ActualValue(bool value) => _invert ? !value : value;
    }

    public readonly partial struct ColorUnion : IUnion<Color> { }
}
