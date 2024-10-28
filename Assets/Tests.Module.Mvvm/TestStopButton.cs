using Module.Core.Mvvm.ComponentModel;
using Module.Core.Mvvm.Input;
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
}
