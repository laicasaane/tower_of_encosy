using EncosyTower.Modules.Mvvm.ComponentModel;
using EncosyTower.Modules.Mvvm.Input;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    public sealed partial class SampleStopButton : MonoBehaviour, IObservableObject
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
