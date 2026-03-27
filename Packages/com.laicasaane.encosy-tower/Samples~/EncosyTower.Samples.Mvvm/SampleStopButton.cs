using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.Input;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    [ObservableObject]
    public sealed partial class SampleStopButton : MonoBehaviour
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
