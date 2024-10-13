using Module.Core.Mvvm.ComponentModel;
using UnityEngine;

namespace Module.MainGame
{
    public sealed partial class WorldLoader : MonoBehaviour, IObservableObject
    {
        [ObservableProperty] public Vector3 Rotation { get => Get_Rotation(); set => Set_Rotation(value); }

        [ObservableProperty] public string Name { get => Get_Name(); set => Set_Name(value); }

        [ObservableProperty] public bool IsActive { get => Get_IsActive(); set => Set_IsActive(value); }
    }
}
